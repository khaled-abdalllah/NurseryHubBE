/* Login password visibility toggle */
(function () {
  var bindToggle = function () {
    var button = document.getElementById("PasswordVisibilityButton");
    if (!button) {
      return;
    }

    var wrap = button.closest(".nh-login-password-wrap");
    if (!wrap) {
      return;
    }

    var input = wrap.querySelector("input.form-control");
    if (!input) {
      return;
    }

    var icon = button.querySelector(".nh-login-visibility-icon");
    var showLabel = button.getAttribute("data-show-label") || "Show password";
    var hideLabel = button.getAttribute("data-hide-label") || "Hide password";

    button.addEventListener("click", function () {
      var isPassword = input.getAttribute("type") === "password";
      input.setAttribute("type", isPassword ? "text" : "password");

      if (icon) {
        icon.textContent = isPassword ? "visibility" : "visibility_off";
      }

      button.setAttribute("aria-label", isPassword ? hideLabel : showLabel);
    });
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", bindToggle);
  } else {
    bindToggle();
  }
})();
