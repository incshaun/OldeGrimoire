'use strict';

const mediaStreamConstraints = {
  video: true,
};

function gotLocalMediaStream(mediaStream) {
  var videoTags = document.getElementsByTagName ("video");
  console.log ("Got " + videoTags.length + " video tags");
  for (var i = 0; i < videoTags.length; i++)
  {
    videoTags[i].srcObject = mediaStream;
  }
}

function handleLocalMediaStreamError(error) {
  console.log('navigator.getUserMedia error: ', error);
}

navigator.mediaDevices.getUserMedia (mediaStreamConstraints) .then(gotLocalMediaStream) .catch(handleLocalMediaStreamError);
