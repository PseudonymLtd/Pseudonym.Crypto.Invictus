window.functions = {

    WriteCookie: function (name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toGMTString();
        }
        var cookie = name + "=" + btoa(value) + expires + "; path=/";
        document.cookie = cookie;
    },
    ReadCookie: function (name) {
        const cname = name + "=";
        const decodedCookie = decodeURIComponent(document.cookie);
        const ca = decodedCookie.split(';');

        for (var i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(cname) == 0) {
                return atob(c.substring(cname.length, c.length));
            }
        }

        return null;
    }
}