/* Dev Login Helper (intentionally disabled) */
(() => {
  const run = () => {
    // Keep this file to avoid missing asset errors in development.
    // Auto-fill and credential hints were removed intentionally.
    return;
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", run);
  } else {
    run();
  }
})();