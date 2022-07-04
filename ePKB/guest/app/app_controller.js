app.controller('homeController', function (authService, $location) {
    if (authService.isAdminLogin())
        $location.path('/admin/list');
});
app.controller('step1Controller', function ($rootScope, $scope, $http, dialogService, appService) {
    $rootScope.currentStep = 1;
    appService.loadOption();
    appService.loadProvinces();
    $scope.resources = {};
    $scope.emailExist = false;
    $scope.checkEmail = false;
    $scope.register = false;
    $scope.selectedVehicle = {};
    $scope.doCheckEmail = function (a) {
        if (!a) {
            dialogService.errorDialog('Silahkan ketik email anda!');
            return;
        }
        var param = {};
        param.email = a;
        $http.post('/api/epkb/checkemail', param).then(function (success) {
            $scope.checkEmail = true;
            if (success.data)
                $scope.emailExist = true;
            else {
                $scope.emailExist = false;
                $scope.register = true;
            }
        });
    }
    $scope.login = function (a) {
        var param = {};
        param.UserName = a.UserProfile.Email;
        param.Password = a.Password;
        $rootScope.authService.login(param).then(function (success) {
            $scope.fillUserData();
        });
    }
    $scope.reset = function () {
        $scope.emailExist = false;
        $scope.checkEmail = false;
        $scope.resources.UserProfile.Email = '';
    }
    $scope.fillUserData = function () {
        appService.loadUser($rootScope.authService.authentication.user.Id).then(function (success) {
            $scope.resources.UserProfile = success.data;
            $scope.logedin = true;
        });
    }
    $scope.save = function (a) {
        if (!$scope.logedin)
            appService.saveRegistration(a).then(function (success) {
                $scope.login(a);
            }, function (error) {
                dialogService.errorDialog(error.data.Message);
            });
        else
            appService.saveData(a);
    }
    $scope.openRegister = function () {
        $scope.register = true;
    };
    $scope.openVehicle = function (a) {
        $scope.selectedVehicle = a;
        try {
            if (a.ExpireSTNKDate) {
                var utc = moment.utc(a.ExpireSTNKDate);
                utc.local();
                a.ExpireSTNKDate = utc;
            }
            $scope.resources.Vehicle = a;
            angular.forEach(appService.Provinces, function (prov) {
                if (a.Province == prov.Name)
                    a.Province = prov;
            });
            angular.forEach(a.Province.Regions, function (reg) {
                if (a.IdRegion == reg.Id)
                    a.Region = reg;
            });
        }
        catch (e) {
            console.log(e)
        }
    }
    if ($rootScope.authService.authentication.isAuth) {
        $scope.fillUserData();
    }
});

app.controller('profileController', function ($scope, $http, appService, authService, $location) {
    var load = function () {
        var param = {};
        param.id = authService.authentication.user.Id;
        $http.post('/api/epkb/userdata', param).then(function (success) {
            $scope.resources = {};
            $scope.resources.UserProfile = success.data;
            $scope.resources.TaxPayments = success.data.TaxPayments;
            $scope.resources.Vehicles = success.data.Vehicles;
        });
    }
    $scope.do = function (a) {
        switch (a.Status) {
            case appService.status_upload:
                $location.path('/step2/' + a.Id);
                break;
            case appService.status_validateData:
            case appService.status_waitForPayment:
                $location.path('/step3/' + a.Id);
                break;
            case appService.status_print:
                $location.path('/step4/' + a.Id);
                break;
            case appService.status_confirmPayment:
                $location.path('/step4/' + a.Id);
                break;
        }
    }
    load();
});

app.controller('step2Controller', function ($rootScope, $scope, $routeParams, uploadService, dialogService, $http, $location, appService) {
    $rootScope.currentStep = 2;
    $scope.sub = $routeParams.sub;
    var id = $routeParams.sub;
    var load = function () {
        var param = {};
        param.id = id;
        $http.post('/api/epkb/taxpayment', param).then(function (success) {
            $scope.resources = success.data;
            $scope.Images = success.data.Images;
            

            param.regnumber = $scope.resources.RegNumber;
            $http.post('/api/epkb/images', param).then(function (success1) {
                $scope.Images = success1.data;
            }, function (error1) {
                swal("Oops!", error1.data.Message, 'info');
            });
        }, function (error) {
            dialogService.error(error.data.Message);
        });
    }
    load();

    $scope.dragDrop = function (a, type) {
        var files = [];
        angular.forEach(a.files, function (b) {
            files.push(b.file);
        });
        uploadService.upload(files, $scope.resources.RegNumber, $scope.resources.RegNumber + "_" + type).then(function (success) {
            load();
        }, function (error) {
            swal("Oops!", error.data.Message, 'info');
        });
        a.files = [];
    }
    $scope.save = function (a) {
        if (!a.ktp || !a.ktp.Path) {
            dialogService.errorDialog('Gambar KTP belum diupload!');
            return;
        }
        if (!a.stnk || !a.stnk.Path) {
            dialogService.errorDialog('Gambar STNK belum diupload!');
            return;
        }
        if (!a.ssp || !a.ssp.Path) {
            dialogService.errorDialog('Gambar Bukti Setor Pajak Tahun Terakhir belum diupload!');
            return;
        }
        if (!a.bpkb || !a.bpkb.Path) {
            dialogService.errorDialog('Gambar BPKB belum diupload!');
            return;
        }
        var param = {};
        param.id = $scope.resources.Id;
        param.status = appService.status_validateData;
        $http.post('/api/epkb/updatestatus', param).then(function (success) {
            $location.path('/step3/' + $scope.resources.Id);
        });
    }
    $scope.deleteImage = function (a) {
        $http.post('/api/file/delete', a).then(function (success) {
            load();
        });
    }
});

app.controller('step3Controller', function ($scope, appService, $http, $rootScope, $routeParams, dialogService, uploadService, $location) {
    $rootScope.currentStep = 3;
    var id = $routeParams.sub;
    $scope.sub = id;
    var load = function () {
        var param = {};
        param.id = id;
        $http.post('/api/epkb/taxpayment', param).then(function (success) {
            $scope.resources = success.data;
            $scope.Images = $scope.resources.Images;
        }, function (error) {
            dialogService.error(error.data.Message);
        });
    }
    $scope.dragDrop = function (a, type) {
        var files = [];
        angular.forEach(a.files, function (b) {
            files.push(b.file);
        });
        uploadService.upload(files, $scope.resources.RegNumber, $scope.resources.RegNumber + "_" + type).then(function (success) {
            $scope.Images = success.data;
        }, function (error) {
            swal("Oops!", error.data.Message, 'info');
        });
        a.files = [];
    }
    load();
    $scope.save = function () {
        var param = {};
        param.id = $scope.resources.Id;
        param.status = appService.status_confirmPayment;
        $http.post('/api/epkb/updatestatus', param).then(function (success) {
            $location.path('/step4/' + $scope.resources.Id);
        }, function (error) {
            dialogService.errorDialog(error.data.Message);
        });
    }
    $scope.deleteImage = function (a) {
        $http.post('/api/file/delete', a).then(function (success) {
            load();
        });
    }
});

app.controller('step4Controller', function ($scope, $rootScope, $routeParams, $http, appService) {
    $rootScope.currentStep = 4;
    $scope.sub = $routeParams.sub;
    var load = function () {
        var param = {};
        param.id = $scope.sub;
        $http.post('/api/epkb/taxpayment', param).then(function (success) {
            $scope.resources = success.data;
        }, function (error) {
            dialogService.error(error.data.Message);
        });
    }
    load();
});

app.controller('userController', function (authService, $scope, $location, $timeout, $http) {
    if ($location.path() == '/forgotpassword') {
        $('#forgotmodal').modal('show');
    }

    $scope.login = function (a) {
        authService.login(a).then(function (success) {
            if (!authService.isAdminLogin()) {
                if (authService.authentication.user.TaxPaymentsCount > 0) {
                    $location.path('/profile');
                }
                else
                    $location.path('/step1');
            }
            else
                $location.path('/admin/list');
        }, function (error) {

        });
    }

    $scope.passwordResetLink = function (a) {
        $http.post('/api/account/passwordResetLink', a).then(function (success) {
            swal({
                title: "Silahkan cek Email anda!",
                text: 'Petunjuk untuk mereset password telah dikirim ke email anda',
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

    $scope.passwordReset = function (a) {
        if (a.Password != a.PasswordConfirm) {
            swal('Oops!', 'Pastikan password anda benar!', 'warning');
            return;
        }

        a.AspNetUser = {};
        a.UserName = $location.search().id;
        a.Token = $location.search().code;
        authService.resetPassword(a).then(function (success) {
            swal({
                title: "Sukses!",
                text: 'Pasword telah direset!',
                type: "success",
                timer: 2000,
                showConfirmButton: false
            },
                function () {
                    $timeout(function () {
                        swal.close();
                        authService.logout(true);
                        $location.path('/');
                    }, 0);
                });
        }, function (error) {
            swal('Sorry!', error.data.Message, 'info');
        });
    }
});

app.controller('adminListController', function ($scope, $rootScope, $http) {
    $rootScope.isAdmin = true;
    var load = function () {
        $http.get('/api/admin/list').then(function (success) {
            $scope.resources = success.data;
        });
    };
    load();
});

app.controller('adminValidateController', function ($scope, $routeParams, $rootScope, $http, appService, dialogService, $location) {
    $rootScope.isAdmin = true;
    $scope.sub = $routeParams.sub;
    var load = function () {
        $scope.v_user = [];
        for (var i = 0; i < 5; i++)
            $scope.v_user.push(false);
        $scope.v_vehicle = [];
        for (var i = 0; i < 14; i++)
            $scope.v_vehicle.push(false);

        var param = {};
        param.id = $scope.sub;
        param.status = appService.status_validateData;
        param.rotc = 'true';
        $http.post('/api/epkb/taxpayment', param).then(function (success) {
            $scope.resources = success.data;
        }, function (error) {
            dialogService.error(error.data.Message);
        });
    };
    load();
    $scope.validate = function (a, b, c, d, e) {
        var isValidUser = true;
        angular.forEach(a, function (vu) {
            if (!vu)
                isValidUser = false;
        });
        if (!isValidUser) {
            dialogService.errorDialog("Semua data pemilik harus dicek terlebih dahulu! Kirim pesan ke pemilik kendaraan bila diperlukan.");
            return;
        }

        var isValidVehicle = true;
        angular.forEach(b, function (vv) {
            if (!vv)
                isValidVehicle = false;
        });
        if (!isValidVehicle) {
            dialogService.errorDialog("Semua data kendaraan harus dicek terlebih dahulu! Kirim pesan ke pemilik kendaraan bila diperlukan.");
            return;
        }
        if (e) {
            var param = {};
            param.id = $scope.resources.Id;
            param.status = e;
            $http.post('/api/epkb/updatestatus', param).then(function (success) {
                $location.path('/admin/list');
            });
            return;
        }
        if (!d) {
            dialogService.errorDialog("Mohon centang pernyataan bahwa anda telah memastikan semua data telah sesuai!");
            return;
        }
        $http.post('/api/admin/validate', c).then(function (success) {
            dialogService.saveSuccess().then(function (d) {
                $location.path('/admin/list');
            });
        }, function (error) {
            dialogService.errorDialog(error.data.Message);
        });

    }
});

app.controller('adminConfirmPaymentController', function ($scope, $routeParams, $rootScope, $http, dialogService, appService, $location) {
    $rootScope.isAdmin = true;
    $scope.sub = $routeParams.sub;
    var load = function () {
        var param = {};
        param.id = $scope.sub;
        $http.post('/api/epkb/taxpayment', param).then(function (success) {
            $scope.resources = success.data;
        }, function (error) {
            dialogService.error(error.data.Message);
        });
    }
    load();
    $scope.save = function (a, b) {
        if (!a) {
            dialogService.errorDialog("Silahkan centang tombol konfirmasi jika anda telah memeriksa dan memastikan dana telah diterima.");
            return;
        }
        var param = {};
        param.id = $scope.resources.Id;
        param.status = b? b : appService.status_print;
        $http.post('/api/epkb/updatestatus', param).then(function (success) {
            $location.path('/admin/list');
        });
    }
});