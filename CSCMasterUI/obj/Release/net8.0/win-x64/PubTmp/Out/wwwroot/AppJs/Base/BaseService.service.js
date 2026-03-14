(function () {
    'use strict';

    angular.module('app').service('server', ['PopupModalService', server]);

    function server(popup) {
        this.Get = Get;
        this.Post = Post;
        function Get(url, sucessFunc) {
            GetPrivate(apiBaseUrl + url, sucessFunc);
        }
        function Post(url, data, sucessFunc, failureFunc) {
            $('.loader_bg').fadeIn();

            var token = GetValueFromCache("logintoken");
            if (url.includes("Auth/Login") || url.includes("Home/Logout") || !isJwtExpired(token))
                doPost(token, url, data, sucessFunc, failureFunc);
            else {
                refreshJwtToken(function () {
                    token = GetValueFromCache("logintoken");
                    doPost(token, url, data, sucessFunc, failureFunc);
                });
            }
        }
        function GetPrivate(url, sucessFunc) {
            $('.loader_bg').fadeIn();

            var token = GetValueFromCache("logintoken");
            if (url.includes("Auth/Login") || url.includes("Home/Logout") || !isJwtExpired(token))
                doGet(token, url, sucessFunc);
            else {
                refreshJwtToken(function () {
                    token = GetValueFromCache("logintoken");
                    doGet(token, url, sucessFunc);
                });
            }
        }
        function doGet(token, url, sucessFunc) {
            $.ajax({
                async: false,
                type: "GET",
                contentType: "application/json",
                url: url,
                headers: {
                    "Authorization": "Bearer " + token
                },
                success: function (response) {
                    setTimeout(function () {
                        $('.loader_bg').fadeOut();
                    }, 10);
                    if (response.success) {
                        sucessFunc(response.sucessValue);
                    }
                    else {
                        const str = response.errorMessage.map(e => {
                            if (e.field && e.fieldValue) {
                                return `${e.field}: ${e.message} (wrong value: ${e.fieldValue})`;
                            } else if (!e.field && e.fieldValue) {
                                return `${e.message} (wrong value: ${e.fieldValue})`;
                            } else if (e.field && !e.fieldValue) {
                                return `${e.field}: ${e.message}`;
                            } else {
                                return `${e.message}`;
                            }
                        }).join('\n');
                        console.log(str);
                        popup.error(str);
                    }
                },
                error: function (response, exception) {
                    setTimeout(function () {
                        $('.loader_bg').fadeOut();
                    }, 10);
                    var msg = '';
                    if (response.status === 0) {
                        msg = 'Not connect.\n Verify Network.';
                    } else if (response.status == 401) {
                        if (url.startsWith(apiBaseUrl) && !url.includes("/Auth/refresh")) {
                            refreshJwtToken(function () {
                                var token = GetValueFromCache("logintoken");
                                doGet(token, url, sucessFunc);
                            });
                        }
                        else if (window.location.pathname !== "/Home/Login") {
                            msg = 'Not authorized request, Please login.';
                            logOut();
                        }
                    } else if (response.status == 404) {
                        msg = 'Requested page not found. [404]';
                    } else if (response.status == 500) {
                        msg = 'Internal Server Error [500].';
                    } else {
                        msg = 'Uncaught Error.\n' + response.responseText;
                    }
                    console.log(msg);
                    if (msg != "")
                        popup.error(msg);
                }
            });
        }
        function doPost(token, url, data, sucessFunc, failureFunc) {
            var isFormData = (data instanceof FormData);

            $.ajax({
                async: false,
                type: "POST",
                url: apiBaseUrl + url,
                data: isFormData ? data : JSON.stringify(data),
                contentType: isFormData ? false : "application/json",
                processData: !isFormData,
                headers: {
                    "Authorization": "Bearer " + token
                },
                success: function (response) {
                    console.log(response);
                    setTimeout(function () {
                        $('.loader_bg').fadeOut();
                    }, 10);

                    if (response.success) {
                        sucessFunc(response.sucessValue);
                    }
                    else {
                        if (failureFunc != null) {
                            failureFunc(response.errorMessage);
                        }
                        else {
                            const str = response.errorMessage.map(e => {
                                if (e.field && e.fieldValue) {
                                    return `${e.field}: ${e.message} (wrong value: ${e.fieldValue})`;
                                } else if (!e.field && e.fieldValue) {
                                    return `${e.message} (wrong value: ${e.fieldValue})`;
                                } else if (e.field && !e.fieldValue) {
                                    return `${e.field}: ${e.message}`;
                                } else {
                                    return `${e.message}`;
                                }
                            }).join('\n');
                            console.log(url + " " + str);
                            popup.error(str);
                        }
                    }
                },
                error: function (response, exception) {
                    setTimeout(function () {
                        $('.loader_bg').fadeOut();
                    }, 10);
                    var msg = '';
                    if (response.status === 0) {
                        msg = 'Not connect.\n Verify Network.';
                    } else if (response.status == 401) {
                        if (url.startsWith(apiBaseUrl) && !url.includes("/Auth/refresh")) {
                            refreshJwtToken(function () {
                                var token = GetValueFromCache("logintoken");
                                doPost(token, url, data, sucessFunc, failureFunc);
                            });
                        }
                        else if (window.location.pathname !== "/Home/Login") {
                            msg = 'Not authorized request, Please login.';
                            logOut();
                        }
                        else
                            msg = 'Wrong Userid/Password';
                    } else if (response.status == 404) {
                        msg = 'Requested page not found. [404]';
                    } else if (response.status == 500) {
                        msg = 'Internal Server Error [500].';
                    } else {
                        msg = 'Uncaught Error.\n' + response.responseText;
                    }
                    console.log(msg);
                    if (msg != "")
                        popup.error(msg);
                }
            });
        }
        function logOut() {
            RemoveFromCache("logintoken");
            window.location.href = "/Home/Login";
        }
        function refreshJwtToken(sucessFun) {
            var currentToken = GetValueFromCache("logintoken");
            var data = {
                accessToken: currentToken,
                refreshToken: GetValueFromCache("refreshtoken")
            };

            if (currentToken != "")
                doPost(currentToken, "/Auth/refresh", data, function (response) {
                    AddIntoCache("logintoken", response.accessToken);
                    AddIntoCache("refreshtoken", response.refreshToken);
                    fetch("/Home/StoreToken", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(response.accessToken)
                    }).then(response => {
                        if (response.ok) {
                            sucessFun();
                        } else {
                            popup.error("Token store failed.");
                        }
                    });
                }, function () {
                    popup.error('Not authorized request, Please login.');
                    logOut();
                });
        }
        function isJwtExpired(token) {
            if (!token) return true;
            try {
                var payload = JSON.parse(atob(token.split('.')[1]));
                var now = Math.floor(Date.now() / 1000);
                return payload.exp < now;
            } catch (e) {
                return true;
            }
        }
    }
})();