(function () {
    window.addEventListener("load", function () {
        setTimeout(function () {
            const logo = document.getElementsByClassName('link')[0];
            const logoUrl = window.location.href.toString().replace("swagger/index.html", "resources/logo.png");
            const faviconUrl = window.location.href.toString().replace("swagger/index.html", "resources/favicon.png");

            logo.children[0].alt = "Pseudonym";
            logo.children[0].src = logoUrl;

            for (let i = 0; i < document.head.children.length; i++) {
                if (document.head.children[i].tagName === "LINK" &&
                    document.head.children[i].rel === "icon") {
                    document.head.children[i].href = faviconUrl;
                }
            }

            document.body.getElementsByClassName('link')[0].firstChild.height = 70;
        });
    });
})();