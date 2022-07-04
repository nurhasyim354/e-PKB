app.config(function ($routeProvider, $locationProvider, constant) {
    $routeProvider
        .when('/', {
            templateUrl: '/Guest/views/home.html?v=' + constant.ver,
            title: 'e-PKB',
            controller: 'homeController',
        })
        .when('/login', {
            templateUrl: '/Guest/views/login.html?v=' + constant.ver,
            title: 'Login',
            controller: 'userController',
        })
        .when('/forgotpassword', {
            templateUrl: '/Guest/views/login.html?v=' + constant.ver,
            name: 'Forgot Password',
            controller: 'userController',
        })
        .when('/update/password', {
            templateUrl: '/Guest/views/update.html?v=' + constant.ver,
            name: 'Update Password',
            controller: 'userController'
        })
        .when('/step1', {
            templateUrl: '/Guest/views/data.html?v=' + constant.ver,
            title: 'Lengkapi Data',
            controller: 'step1Controller',
        })
        .when('/step2/:sub', {
            templateUrl: '/Guest/views/upload.html?v=' + constant.ver,
            title: 'Upload Dokumen',
            controller: 'step2Controller',
        })
        .when('/step3/:sub', {
            templateUrl: '/Guest/views/payment.html?v=' + constant.ver,
            title: 'Pembayaran',
            controller: 'step3Controller',
        })
        .when('/step4/:sub', {
            templateUrl: '/Guest/views/finish.html?v=' + constant.ver,
            title: 'Ambil Bukti Setor Pajak',
            controller: 'step4Controller',
        })
        .when('/profile', {
            templateUrl: '/Guest/views/profile.html?v=' + constant.ver,
            title: 'Profile Pengguna',
            controller: 'profileController',
        })
        .when('/admin/list', {
            templateUrl: '/Guest/views/admin_list.html?v=' + constant.ver,
            title: 'Daftar Pembayaran PKB',
            controller: 'adminListController',
            settings: {
                rolesPermitted : ['admin']
            }
        })
        .when('/admin/validatedata/:sub', {
            templateUrl: '/Guest/views/admin_validatedata.html?v=' + constant.ver,
            title: 'Validasi Data',
            controller: 'adminValidateController',
            settings: {
                rolesPermitted: ['admin']
            }
        })
        .when('/admin/confirmpayment/:sub', {
            templateUrl: '/Guest/views/admin_confirmpayment.html?v=' + constant.ver,
            title: 'Konfirmasi Pembayaran',
            controller: 'adminConfirmPaymentController',
            settings: {
                rolesPermitted: ['admin']
            }
        })
        .when('/404', {
            templateUrl: '/Guest/views/404.html?v=' + constant.ver,
            title: 'Page not Found',
        })
        .otherwise({
            redirectTo: '/404',
        });

    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });
});

app.run(function ($rootScope, $anchorScroll, $location, constant, authService, appService) {
    $rootScope.constant = constant;
    $rootScope.authService = authService;
    $rootScope.appService = appService;
    $rootScope.isLoading = false;

    $rootScope.$on('$routeChangeStart', function (event, current, previous) {
        $rootScope.isLoading = true;
        $rootScope.isAdmin = false;
        if (current.originalPath) {
            previousPath = current.originalPath.split(":");
        }
        var isAllowed = false;
        if (current.settings && current.settings.rolesPermitted && current.settings.rolesPermitted.length > 0)
        {
            if (authService.authentication.isAuth) {
                angular.forEach(authService.authentication.user.Roles, function (role) {
                    isAllowed = $.inArray(role, current.settings.rolesPermitted) > -1;
                });
            };
        }
        else
            isAllowed = true;

        if (!isAllowed) {
            $location.path('/login').search('returnUrl', previousPath[0]);
        }
       
    });

    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
        $rootScope.isLoading = false;
        $rootScope.activeTab = current.activeTab ? current.activeTab : $location.path();
        $rootScope.title = current.title;
        $anchorScroll();

        //Hide Navigation on mobile
        if ($('nav .navbar-collapse').hasClass('in')) {
            $("nav .navbar-toggle").trigger("click");
        }
        swal.close();
        $('.modal').modal('hide');
    });
});