//=============================INTERCEPT HTTP=============================
app.factory('httpInterceptor', function ($q, $rootScope) {
    var loadingCount = 0;
    return {
        request: function (config) {
            if (++loadingCount === 1) $rootScope.$broadcast('app:progress', config);
            return config || $q.when(config);
        },

        response: function (response) {
            if (--loadingCount === 0) $rootScope.$broadcast('app:finish', response);
            return response || $q.when(response);
        },

        responseError: function (response) {
            if (--loadingCount === 0) $rootScope.$broadcast('app:error', response);
            //$rootScope.$broadcast('app:error', response);
            return $q.reject(response);
        }
    };
}).config(function ($httpProvider) {
    $httpProvider.interceptors.push('httpInterceptor');
});
app.run(function ($rootScope, $timeout, ngProgressLite, authService) {
    $rootScope.globalLoading = true;

    var timer;
    $rootScope.$on('app:progress', function (event, data) {
        ngProgressLite.start();
        $timeout.cancel(timer);
        timer = $timeout(function () {
            if (data.url.substring(0, 14) != '/api/postcode/'
            && data.url != "/api/cart"
            && data.url != "/api/card"
            && data.url != "/api/shipping"
            && data.url != "/api/shipping/loadconfig"
            && data.url != "/api/tax"
            && data.url != "/api/cart/validate"
            )
                $rootScope.transition = true;
        }, 200);
    });

    $rootScope.$on('app:finish', function (event, data) {
        $timeout.cancel(timer);
        timer = $timeout(function () {
            ngProgressLite.done();
            $rootScope.globalLoading = false;
            $rootScope.transition = false;
            $rootScope.firstLoading = true;
        }, 500);
    });

    $rootScope.$on('app:error', function (event, response) {
        $timeout.cancel(timer);
        timer = $timeout(function () {
            ngProgressLite.done();
            $rootScope.globalLoading = false;
            $rootScope.transition = false;
            $rootScope.firstLoading = true;
        }, 500);

        if (response && (response.status != 401 && response.status != 403)) {
            var errorTitle = "Connection Lost";
            var errorMessage = "Please check your internet connection and try reload the page again";
            try {
                if (angular.isDefined(response.data)) {
                    !response.data.error ? errorTitle = response.statusText : errorTitle = response.data.error;
                    !response.data.error_description ? errorMessage = response.data.Message : errorMessage = response.data.error_description;
                }
            } catch (e) { }

            //show error on connection lost, etc
            if (errorTitle == "Bad Request")
                errorTitle = "Oops";

            if (errorMessage != "Please check your internet connection and try reload the page again")
                errorMessage = errorMessage;

            swal(errorTitle, errorMessage, "warning");
        }

        if (response && response.status == 401) {
            authService.logout();
        }
    });
});