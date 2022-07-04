//---------------------------------------AUTHSERVICE---------------------------------------
app.run(['authService', function (authService) {
    authService.fillAuthData();
}]);
'use strict';
app.factory('authService', function ($http, $location, $q) {
    var factory = {};
    var _authentication = {
        isAuth: false,
        user: {}
    };
    var _saveRegistration = function (model) {
        _logout(true);
        var deferred = $q.defer();
        $http.post('/api/account/register', model).then(function (response) {
            _login(model).then(function (success) {
                deferred.resolve(success.data);
            }, function (error) {
                deferred.reject(error);
            });
        }, function (error) {
            deferred.reject(error);
        });
        return deferred.promise;
    };
    var _saveGuest = function (model) {
        _logout(true);
        var def = $q.defer();
        $http.post('/api/account/guest', model).then(function (response) {
            _authentication.isAuth = true;
            _authentication.isGuest = true;
            _authentication.user = response.data;
            _authentication.user.Roles = [];
            _authentication.user.Roles.push('guest');
            _savedata();
            def.resolve(response);
        }, function (error) {
            def.reject(error);
        });
        return def.promise;
    };
    var _login = function (model) {
        var deferred = $q.defer();
        var data = "grant_type=password&username=" + encodeURIComponent(model.UserName) + "&password=" + encodeURIComponent(model.Password);
        $http.post('/token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).then(function (success) {
            localStorage.setItem('authorizationData', JSON.stringify(success.data));
            _fillAuthData();
            deferred.resolve(success.data);
        }, function (err) {
            deferred.reject(err);
        });
        return deferred.promise;
    };
    var _savedata = function () {
        var authdata = {};
        authdata.user = JSON.stringify(_authentication.user);
        localStorage.setItem('authorizationData', JSON.stringify(authdata));
    };
    var _logout = function (a) {
        var deferred = $q.defer();
        localStorage.removeItem('authorizationData');
        sessionStorage.clear();
        _authentication.isAuth = false;
        _authentication.user = {};
        deferred.resolve("Successfully logged out");
        if (!a)
            $location.path('/');
        else
            $location.path(a);
        return deferred.promise;
    };
    var _fillAuthData = function () {
        _authentication.isGuest = true;
        var authData = JSON.parse(localStorage.getItem('authorizationData'));
        if (authData) {
            _authentication.user = JSON.parse(authData.user);
        }
        if (_authentication.user && _authentication.user.Roles && _authentication.user.Roles.length > 0) {
            _authentication.isGuest = _authentication.user.Roles.indexOf('guest') != -1;
            _authentication.isAuth = true;
        }
        else
            _authentication.isAuth = false;
    };
    var _resetPassword = function (a) {
        var d = $q.defer();
        if (a.Password != a.PasswordConfirm) {
            d.reject('Password and Confirm Password field are not identical');
            return d.promise;
        }
        a.AspNetUser = {};
        a.AspNetUser.Email = $location.search().id;
        a.Token = $location.search().code;
        var url = '/api/account/passwordreset';
        $http.post(url, a).then(function (response) {
            d.resolve(response);
        }, function (error) {
            d.reject(error);
        });
        return d.promise;
    };
    var _resetPasswordLink = function (a) {
        $http.post('/api/account/passwordResetLink', a).then(function (success) {
            swal({
                title: "Please check your email!",
                text: 'Link to reset your password has been emailed to you',
                type: "success",
            },
                function () {
                    swal.close();
                    $('.modal').modal('hide');
                });
        }, function (error) {
            swal('Sorry!', error.data.Message, 'info');
        });
    }
    var _isAdminLogin = function () {
        if (!_authentication.isAuth || !_authentication.user || _authentication.user.Roles.length == 0)
            return false;

        var res = false;
        angular.forEach(factory.authentication.user.Roles, function (role) {
            if (role == 'admin')
                res = true;
        });
        return res;
    }
    factory.resetPasswordLink = _resetPasswordLink;
    factory.resetPassword = _resetPassword;
    factory.saveRegistration = _saveRegistration;
    factory.login = _login;
    factory.fillAuthData = _fillAuthData;
    factory.saveGuest = _saveGuest;
    factory.logout = _logout;
    factory.authentication = _authentication;
    factory.isAdminLogin = _isAdminLogin;
    return factory;
});

'use strict';
app.factory('authInterceptorService', function ($q, $injector, $rootScope, $location) {
    var authInterceptorServiceFactory = {};
    var _request = function (config) {
        config.headers = config.headers || {};
        var authData = JSON.parse(localStorage.getItem('authorizationData'));
        if (authData)
            config.headers.Authorization = 'Bearer ' + authData.access_token;
        $rootScope.$broadcast('app:progress', config);
        return config;
    }
    var _responseError = function (rejection) {
        if (rejection.status === 401) {
            var authService = $injector.get('authService');
            var authData = JSON.parse(localStorage.getItem('authorizationData'));
            var originalPath = $location.path();
            $rootScope.$broadcast('app:error', rejection);
            $location.path('/login').search('returnUrl', encodeURIComponent(originalPath));
        }
        return $q.reject(rejection);
    }
    authInterceptorServiceFactory.request = _request;
    authInterceptorServiceFactory.responseError = _responseError;
    return authInterceptorServiceFactory;
}).config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});