/* Login password visibility toggle */
(function () {
  var bindToggle = function () {
    var button = document.getElementById("PasswordVisibilityButton");
    if (button) {
      var wrap = button.closest(".nh-login-password-wrap");
      if (wrap) {
        var input = wrap.querySelector("input[name$='Password'], input[id$='Password']");
        if (input) {
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
        }
      }
    }

    var loginForm = document.getElementById("LoginForm");
    var nurseryInput = document.getElementById("NurseryCodeInput");
    if (!loginForm || !nurseryInput) {
      return;
    }

    loginForm.addEventListener("submit", function () {
      var tenantValue = (nurseryInput.value || "").trim();
      var action = loginForm.getAttribute("action") || window.location.pathname + window.location.search;
      var actionUrl = new URL(action, window.location.origin);

      if (tenantValue) {
        actionUrl.searchParams.set("__tenant", tenantValue);
      } else {
        actionUrl.searchParams.delete("__tenant");
      }

      loginForm.setAttribute("action", actionUrl.pathname + actionUrl.search);
    });
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", bindToggle);
  } else {
    bindToggle();
  }
})();
