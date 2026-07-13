(function () {
  const form = document.querySelector("[data-ajax-user-filter]");
  if (!form) {
    return;
  }

  const targetSelector = form.getAttribute("data-ajax-target");

  async function loadUsers(url, pushState) {
    const target = document.querySelector(targetSelector);
    if (!target) {
      window.location.href = url;
      return;
    }

    target.classList.add("ajax-loading");

    try {
      const response = await fetch(url, {
        headers: {
          "X-Requested-With": "XMLHttpRequest"
        }
      });

      if (!response.ok) {
        throw new Error("Request failed");
      }

      const html = await response.text();
      target.outerHTML = html;

      if (pushState) {
        window.history.pushState({}, "", url);
      }
    } catch {
      window.location.href = url;
    }
  }

  function buildUrlFromForm() {
    const formData = new FormData(form);
    const searchParams = new URLSearchParams();

    for (const [key, value] of formData.entries()) {
      if (value) {
        searchParams.set(key, value.toString());
      }
    }

    return `${form.action}?${searchParams.toString()}`;
  }

  form.addEventListener("submit", function (event) {
    event.preventDefault();
    loadUsers(buildUrlFromForm(), true);
  });

  form.querySelectorAll("select").forEach(function (select) {
    select.addEventListener("change", function () {
      loadUsers(buildUrlFromForm(), true);
    });
  });

  document.addEventListener("click", function (event) {
    const link = event.target.closest("[data-ajax-pagination] a");
    if (!link) {
      return;
    }

    event.preventDefault();
    loadUsers(link.href, true);
  });

  window.addEventListener("popstate", function () {
    loadUsers(window.location.href, false);
  });
})();
