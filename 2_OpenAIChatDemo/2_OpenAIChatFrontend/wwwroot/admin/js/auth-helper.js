function getJwtToken() {
    return localStorage.getItem("jwtToken");
}

function parseJwt(token) {
    try {
        let base64Url = token.split('.')[1];
        let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        let jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

function isTokenExpired(token) {
    let payload = parseJwt(token);
    if (!payload || !payload.exp) return true;

    let expiry = payload.exp * 1000; // convert to ms
    return Date.now() > expiry;
}

function ensureAuthenticated() {
    let token = getJwtToken();
    if (!token || isTokenExpired(token)) {
        localStorage.removeItem("jwtToken");
        window.location.href = "/Account/Login";
        return false;
    }
    return true;
}
