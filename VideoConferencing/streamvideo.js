'use strict';

// The peer connection.
var pc;
// The local media stream.
var localStream;
// The media stream from the remote device.
var remoteStream;

// Individual elements on the web page.
var localVideo = document.getElementById('localVideo');
var remoteVideo = document.getElementById('remoteVideo');
var sendOfferButton = document.getElementById('sendOfferButton');
var offerList = document.getElementById('offerList');
var answerOfferButton = document.getElementById('answerOfferButton');
var hangupButton = document.getElementById('hangupButton');
var muteAudioButton = document.getElementById('muteAudioButton');
var muteVideoButton = document.getElementById('muteVideoButton');
var activeOffer;
var dataChannelSend = document.getElementById('dataChannelSend');
var dataChannelReceive = document.getElementById('dataChannelReceive');
var sendMessageButton = document.getElementById('sendMessageButton');
var dataChannel;

// Connect the call control buttons.
sendOfferButton.onclick = sendOffer;
answerOfferButton.onclick = sendAnswer;
hangupButton.onclick = hangup;
muteAudioButton.onclick = function () { localStream.getAudioTracks()[0].enabled = !localStream.getAudioTracks()[0].enabled };
muteVideoButton.onclick = function () { localStream.getVideoTracks()[0].enabled = !localStream.getVideoTracks()[0].enabled };
sendMessageButton.onclick = sendMessageData;

// The signalling socket, used for establishing connections.
var socket = new WebSocket ("wss://10.1.1.4:5000/");
socket.onmessage = receiveMessage;

// First establish access to the local camera/microphone when the page loads.
navigator.mediaDevices.getUserMedia({
  audio: true,
  video: true
})
.then(gotStream)
.catch(function(e) {
  alert('getUserMedia() error: ' + e.name);
});

function initializePeerConnection ()
{
  // 1. Create a new RTC Peer Connection
  //  argument used to specify STUN and TURN servers.
  //  additional effort is required to manage these.
  pc = new RTCPeerConnection(null);
  // 2. Set the ice candidate event handler.
  pc.onicecandidate = handleIceCandidate;
  pc.onaddstream = handleRemoteStreamAdded;
  pc.onremovestream = handleRemoteStreamRemoved;
  
  // For an additional data channel.
  dataChannel = pc.createDataChannel ("MessageChannel");
  dataChannel.onopen = onDataChannelStateChange;
  dataChannel.onclose = onDataChannelStateChange;
  pc.ondatachannel = receiveChannelCallback;
}

// Once access to the camera/microphone are achieved, then connect
// to the signalling server and make an offer.
function gotStream(stream) {
  console.log('Adding local stream.');
  
  // Attach the media stream to an output on the local page.
  localStream = stream;
  localVideo.srcObject = stream;
  
  // Now advertise existence to the signalling server.
  initializePeerConnection ();
  
  // 3. Add the local media stream.
  pc.addStream(localStream);
}

// Send an offer to the signalling server, to be shared with other connected parties.
function sendOffer() {
  pc.createOffer(setLocalAndSendMessage, handleCreateOfferError);
}

function sendAnswer() {
  // 4. Press the answer once an incoming offer has been received.
  pc.setRemoteDescription (new RTCSessionDescription(activeOffer));

  console.log('Sending answer to peer.');
  pc.createAnswer().then(
    setLocalAndSendMessage,
    onCreateSessionDescriptionError
  );
}

function hangup ()
{
  dataChannel.close ();
  pc.close ();
  initializePeerConnection ();
}

// Show all incoming offers as a list, with option to check one.
function displayReceivedOffer (offer)
{
  var x = document.createElement ("input");
  x.onclick = function () { activeOffer = offer; console.log ("Setting acti ", activeOffer) };
  x.setAttribute("type", "radio");
  x.setAttribute("name", "offers");
  x.setAttribute("checked", "checked");
  activeOffer = offer;
  
  var node = document.createElement ("li");
  node.appendChild (x);
  var textnode=document.createTextNode (offer.sdp.toString ().substring (0, 50));
  node.appendChild(textnode);
  offerList.appendChild(node);
}

// Receive a message from the signalling server.
function receiveMessage (event) {
  var message = JSON.parse (event.data);
    // able to handle incoming messages.
    if (message.type == "offer")
    {
      // display incoming offers to the user.
      displayReceivedOffer (message);
    }
    else if (message.type === 'answer')
    {
      // if an offer is answered, then set up the connection.
      pc.setRemoteDescription(new RTCSessionDescription(message));
    }
    // any candidate address updates are registered with the peer connection.
    else if (message.type === 'candidate')
    {
      var candidate = new RTCIceCandidate({
        sdpMLineIndex: message.label,
        candidate: message.candidate
      });
      pc.addIceCandidate(candidate);
    }
}

// Send a message to the signalling server.
function sendMessage (message)
{
  socket.send (JSON.stringify (message));
}

// When new addressing options are available, these are
// shared via the signalling server.
function handleIceCandidate(event) {
  console.log('icecandidate event: ', event);
  if (event.candidate) {
    sendMessage({
      type: 'candidate',
      label: event.candidate.sdpMLineIndex,
      id: event.candidate.sdpMid,
      candidate: event.candidate.candidate
    });
  }
}

// 5. Called when remote stream is added, and this
// is connected to the remote media element on the web page.
function handleRemoteStreamAdded(event) {
  console.log('Remote stream added.');
  remoteStream = event.stream;
  remoteVideo.srcObject = remoteStream;
}

// When an offer is created, store it and share with signalling server.
function setLocalAndSendMessage(sessionDescription) {
  pc.setLocalDescription(sessionDescription);
  sendMessage (sessionDescription);
}

// Called when the data channel becomes active.
function onDataChannelStateChange() {
  if (dataChannel.readyState === 'open') {
    console.log ("Data channel open");
    dataChannelSend.disabled = false;
    dataChannelSend.focus();
    sendMessageButton.disabled = false;
  } else {
    dataChannelSend.disabled = true;
    sendMessageButton.disabled = true;
  }
}

// A new connection that has a data channel has been
// created. Set up a function to handle incoming messages.
function receiveChannelCallback(event) {
  console.log('Receive Channel Callback');
  event.channel.onmessage = onReceiveMessageCallback;
}

// Send a message on the data channel when the send
// button is pressed.
function sendMessageData() {
  var data = dataChannelSend.value;
  dataChannel.send(data);
  console.log('Sent Data: ' + data);
}

// Any incoming data messages are placed in the
// received message box.
function onReceiveMessageCallback(event) {
  console.log('Received Message');
  dataChannelReceive.value = event.data;
}

function handleRemoteStreamRemoved(event) {
  console.log('Remote stream removed. Event: ', event);
}

function handleCreateOfferError(event) {
  console.log('createOffer() error: ', event);
}

function onCreateSessionDescriptionError(error) {
  console.log('Failed to create session description: ' + error.toString());
}

