document.addEventListener("DOMContentLoaded", () => {
  const forms = document.querySelectorAll("form[data-prevent-double-submit='true']");

  forms.forEach((form) => {
    form.addEventListener("submit", () => {
      const submitButtons = form.querySelectorAll("button[type='submit']");

      submitButtons.forEach((button) => {
        if (button.disabled) {
          return;
        }

        const submitText = button.getAttribute("data-submit-text");
        if (submitText) {
          button.dataset.originalText = button.textContent ?? "";
          button.textContent = submitText;
        }

        button.disabled = true;
      });
    });
  });
});
