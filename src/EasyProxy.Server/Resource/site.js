class Common {
    static setStorage(key, value) {
        window.localStorage.setItem(key, value);
    }

    static removeStorage(key) {
        window.localStorage.removeItem(key);
    }

    static getStorage(key) {
        return window.localStorage.getItem(key);
    }
}

class HttpRequestHelper {

    static TOKEN_KEY = "dashboard-token";

    static post(url, data) {
        var headers = new Headers();
        if (Common.getStorage(HttpRequestHelper.TOKEN_KEY)) {
            headers.append('Authorization', Common.getStorage(HttpRequestHelper.TOKEN_KEY));
        }
        return fetch(url, {
            method: 'POST',
            body: JSON.stringify(data),
            headers: headers
        }).then(res => this.processResult(res));
    }

    static get(url) {
        var headers = new Headers();
        if (Common.getStorage(HttpRequestHelper.TOKEN_KEY)) {
            headers.append('Authorization', Common.getStorage(HttpRequestHelper.TOKEN_KEY));
        }
        return fetch(url, {
            method: 'GET',
            headers: headers
        }).then(res => this.processResult(res));
    }

    static processResult(res) {
        if (res.status === 200) {
            return res.json();
        }

        if (res.status === 403) {
            Common.removeStorage(HttpRequestHelper.TOKEN_KEY);
            console.log('登陆跳转');
            window.location = "/dashboard/login";
            return;
        }
    }
}