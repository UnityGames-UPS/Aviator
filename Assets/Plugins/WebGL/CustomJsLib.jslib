mergeInto(LibraryManager.library, {
  SendLogToReactNative: function (messagePtr) {
    try {
      var message = UTF8ToString(messagePtr);
      if (typeof window !== "undefined" && window.ReactNativeWebView) {
        if (typeof window.ReactNativeWebView.postMessage !== "undefined" && window.ReactNativeWebView.postMessage) {
          window.ReactNativeWebView.postMessage(message);
        }
      }
    } catch (e) {
      console.error("[CustomJsLib] SendLogToReactNative Error:", e);
    }
  },

  SendPostMessage: function (messagePtr) {
    try {
      var message = UTF8ToString(messagePtr);
      console.log('sending msg: ', message);
      if (typeof window !== "undefined" && window.ReactNativeWebView) {
        if (typeof window.ReactNativeWebView.postMessage !== "undefined" && window.ReactNativeWebView.postMessage) {
          console.log("Inside ReactNativeWebView");
          if(message == "authToken"){
            window.ReactNativeWebView.postMessage("if message is authtoken");
            var injectedObjectJson = window.ReactNativeWebView.injectedObjectJson();
            var injectedObj = JSON.parse(injectedObjectJson);

            window.ReactNativeWebView.postMessage('Injected obj : ' + injectedObjectJson);
            
            var combinedData = JSON.stringify({
                socketURL: injectedObj.socketURL.trim(),
                cookie: injectedObj.token.trim(),
                nameSpace: injectedObj.nameSpace ? injectedObj.nameSpace.trim() : ""
            });

            if (typeof SendMessage === 'function') {
              SendMessage('SocketManager', 'ReceiveAuthToken', combinedData);
            }
          }
          window.ReactNativeWebView.postMessage(message);
        }
      } 
      else if (typeof window !== "undefined" && window.parent) {
        if (typeof window.parent.postMessage === "function"){
          console.log("Calling window.parent.postMessage");
          window.parent.postMessage({ 
            type: message,
            data: { }
          }, "*");
        }
      }
    } catch (e) {
      console.error("[CustomJsLib] SendPostMessage Error:", e);
    }
  }
  ,

  CopyTextToClipboard: function (textPtr) {
    try {
      var text = UTF8ToString(textPtr);
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(
          function () {
            if (typeof SendMessage === 'function') {
              SendMessage('PROVABLY FAIR - MANAGER', 'OnCopySuccess');
            }
          },
          function (err) {
            console.log("[CustomJsLib] CopyTextToClipboard failed:", err);
          }
        );
      } else {
        console.log("[CustomJsLib] Clipboard API not available.");
      }
    } catch (e) {
      console.error("[CustomJsLib] CopyTextToClipboard Error:", e);
    }
  }
});
