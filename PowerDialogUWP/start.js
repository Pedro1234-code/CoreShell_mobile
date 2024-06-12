(function () {
    "use strict";

    // Initialize the application view
    var ApplicationView = Windows.UI.ViewManagement.ApplicationView;
    var ApplicationViewWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode;

    // Set the preferred launch windowing mode to full screen
    ApplicationView.preferredLaunchWindowingMode = ApplicationViewWindowingMode.fullScreen;

    // Disable layout scaling so Xbox One displays at 100% scale instead of 150%.
    try {
        Windows.UI.ViewManagement.ApplicationViewScaling.trySetDisableLayoutScaling(true);
    } catch (e) { } // not present on all Win10 desktops

    // Disable overscan for Xbox One.
    var applicationView = ApplicationView.getForCurrentView();
    applicationView.setDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.useCoreWindow);

    // Disable emulated mouse mode on Xbox One.
    navigator.gamepadInputEmulation = "keyboard";

    window["cr_windows10"] = true;

    // Create new runtime using the c2canvas
    cr_createRuntime("c2canvas");

    // Attempt to enter full screen mode
    applicationView.tryEnterFullScreenMode();
})();

// Size the canvas to fill the browser viewport.
jQuery(window).resize(function () {
    cr_sizeCanvas(jQuery(window).width(), jQuery(window).height());
});

// Pause and resume on page becoming visible/invisible
function onVisibilityChanged() {
    if (document.hidden || document.mozHidden || document.webkitHidden || document.msHidden)
        cr_setSuspended(true);
    else
        cr_setSuspended(false);
};

document.addEventListener("visibilitychange", onVisibilityChanged, false);
document.addEventListener("mozvisibilitychange", onVisibilityChanged, false);
document.addEventListener("webkitvisibilitychange", onVisibilityChanged, false);
document.addEventListener("msvisibilitychange", onVisibilityChanged, false);
