mergeInto(LibraryManager.library, {
  CreateInvisibleInput: function () {
    if (typeof window === 'undefined') return;

    let input = document.getElementById('unityHiddenInput');
    if (input) return;

    input = document.createElement('input');
    input.id = 'unityHiddenInput';
    input.type = 'text';
    input.style.position = 'absolute';
    input.style.opacity = '0';
    input.style.height = '0';
    input.style.width = '0';
    input.style.border = 'none';
    input.style.outline = 'none';
    input.autocapitalize = 'off';
    input.autocomplete = 'off';
    input.autocorrect = 'off';
    input.spellcheck = false;

    document.body.appendChild(input);

    // Listen for input and send back to Unity
    input.addEventListener('input', function () {
      const value = input.value;
      SendMessage('JS', 'OnKeyboardInput', value);
    });

    // When user hits Enter
    input.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.keyCode === 13) {
        e.preventDefault();
        SendMessage('JS', 'OnKeyboardSubmit');
      }
    });
  },

  FocusInvisibleInput: function () {
    const input = document.getElementById('unityHiddenInput');
    if (input) {
      input.focus();
    }
  },

  BlurInvisibleInput: function () {
    const input = document.getElementById('unityHiddenInput');
    if (input) {
      input.blur();
    }
  },
  
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
          window.ReactNativeWebView.postMessage(message);
        }
      } 
      else if (typeof window !== "undefined" && window.parent) {
        if (typeof window.parent.dispatchReactUnityEvent !== "undefined" && window.parent.dispatchReactUnityEvent) {
          window.parent.dispatchReactUnityEvent(message);
        }
      }
    } catch (e) {
      console.error("[CustomJsLib] SendPostMessage Error:", e);
    }
  }
});
