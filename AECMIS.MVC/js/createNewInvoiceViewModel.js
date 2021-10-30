ko.bindingHandlers.parentvalElement = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var t = valueAccessor();
        var valueIsValid = valueAccessor().isValid();//&& valueAccessor().outstandingPaymentsIsValid(); //&& viewModel.isAnyMessageShown()
        if (!valueIsValid) {
            $(element).addClass("rowError");
        } else {
            $(element).removeClass("rowError");
        }
    }
};

var studentAttendance = function (studentAttendanceViewModel) {

    var self = this, data;

    data = $.extend({ Subjects: [], PaymentPlans: [], SessionAttendanceViewModel: {}, PaymentRequiredPlanId: "", PaymentRequired: "", FirstName: "", LastName: "" },
        studentAttendanceViewModel);
    self.SessionAttendanceViewModel = ko.validatedObservable(new subjectAttendance(data.SessionAttendanceViewModel));
    self.FirstName = data.FirstName;
    self.LastName = data.LastName;
    self.StudentNo = data.StudentNo;
    self.Subjects = data.Subjects;
    self.StudentId = data.StudentId;

    self.Name = ko.computed(function () {
        return self.FirstName + " " + self.LastName;
    });

};

var createNewInvoiceViewModel = function (newInvoiceViewModel) {
    var self = this, data;
        
    data = $.extend({
        SessionAttendees: [],
        isOpen: false,
        StudentSearchViewModel: {},
        SelectedSessionAttendance: {},
        IsSearchDialogOpen: false,
        IsPaymentsDialogOpen: false,
        SaveStatus: {},
        IsSaveStatusDialogOpen: false,
        IsNewInvoiceSaveStatusDialogOpen: false,
        IsAdminDialogOpen: false,
        ValidateAdminPasswordViewModel: {},
        ConfirmationViewModel: {}
    }, newInvoiceViewModel);

    self.StudentId = ko.observable();
    self.StudentNo = ko.observable();
    self.FirstName = ko.observable();
    self.LastName = ko.observable();
    self.IsPaymentRequired = ko.observable(true);
    self.ShowBlock = ko.observable(false);
    self.lstPaymentPlanMain = ko.observableArray();
    self.lstPaymentTypeMain = ko.observableArray();
    self.PaymentPlanSelectedMain = ko.observable().extend({required:true});
    self.PaymentTypeSelectedMain = ko.observable().extend({ required: true });
    self.ChequeNoMain = ko.observable().extend({required: {message: "ChequeNo is required when payment type is cheque",onlyIf: function () {
                return self.PaymentTypeSelectedMain() == 3002;
            }
        }
    });
    self.PaymentDateMain = ko.observable().extend({ required: true });
    self.FieldApplied = ko.observable(false);
    self.AdminURL = ko.observable();
    self.LoadURL = ko.observable();
    self.DataDiscount = ko.observable();
    self.VATAmountBreakdown = ko.observable();    

    ko.mapping.fromJS(data, {

        GetRegisterTemplateUrl: {
            create: function (options) {
                return options.data;
            }
        },
        GetStudentsStudyPlansUrl: {
            create: function (options) {
                return options.data;
            }
        },
        SessionAttendees: {
            create: function (options) {
                return new studentAttendance(options.data);
            }
        },
        StudentSearchViewModel: {
            create: function (options) {
                return new StudentSearchViewModel(options.data, self.AddStudentsToRegister);
            }
        },
        SelectedSessionAttendance: {
            create: function (options) {
                return ko.observable();
            }
        },
        SaveStatus: {
            create: function (options) {
                return ko.observable();
            }
        },
        ValidateAdminPasswordViewModel: {
            create: function (options) {
                return new ValidateAdminPasswordViewModel(options.data);
            }
        },
        ConfirmationViewModel: {
            create: function (options) {
                return new ConfirmationViewModel(options.data);
            }
        }
    }, self);

    //isOpen: ko.observable(false);
    self.OpenSearchDialog = function () {
        self.StudentSearchViewModel.SelectedStudents.removeAll();
        self.StudentSearchViewModel.Students.removeAll();
        this.IsSearchDialogOpen(true);
    };


    self.DataDiscount.subscribe(function (newValue) {

        $.post(self.GetVATData(), { paymentDate: self.PaymentDateMain(), paymentPlan: self.PaymentPlanSelectedMain(), discount: self.DataDiscount() }, function (returnedData) {

            self.VATAmountBreakdown(returnedData);
        }, "json");

    });


    self.PaymentDateMain.subscribe(function (newValue) {

        $.post(self.GetVATData(), { paymentDate: self.PaymentDateMain(), paymentPlan: self.PaymentPlanSelectedMain(), discount: self.DataDiscount() }, function (returnedData) {

            self.VATAmountBreakdown(returnedData);
        }, "json");

    });

    self.PaymentPlanSelectedMain.subscribe(function (newValue) {

        $.post(self.GetVATData(), { paymentDate: self.PaymentDateMain(), paymentPlan: self.PaymentPlanSelectedMain(), discount: self.DataDiscount() }, function (returnedData) {

            self.VATAmountBreakdown(returnedData);
        }, "json");

    });

    self.CloseSearchDialog = function () {
        this.IsSearchDialogOpen(false);
    };

    self.ValidateAdminPassword = function () {                
                jQuery.ajax({
                    url: '/Payment/StudentProcessNewInvoice',
                    type: "POST",
                    data: {
                        StudentId: self.StudentId(),
                        PaymentType: self.PaymentTypeSelectedMain(),
                        PaymentPlan: self.PaymentPlanSelectedMain(),
                        ChequeNo: self.ChequeNoMain(),
                        PaymentDate: self.PaymentDateMain(),
                        Discount: self.DataDiscount(),
                        VATAmount: self.VATAmountBreakdown()
                    },
                    success: function (returnedData) {
                        self.LoadURL(returnedData);
                        self.IsNewInvoiceSaveStatusDialogOpen(true);
                    }
                });
                
    };

    self.ActionOnSelectedStudents = function () {
        if (self.StudentSearchViewModel.SelectedStudents().length < 1) return;
        if (self.CallBack != null && self.CallBack != undefined) {
            self.CallBack(self.StudentSearchViewModel.SelectedStudents());
        }

        $.post('/Payment/GetStudentNewInvoiceDetails', { Id: self.StudentSearchViewModel.SelectedStudents() }, function (returnedData) {

            //$root.SessionAttendees(returnedData);
            self.ShowBlock(true);
            self.StudentId(returnedData.StudentId);
            self.StudentNo(returnedData.StudentNo);
            self.FirstName(returnedData.FirstName);
            self.LastName(returnedData.LastName);
            self.lstPaymentPlanMain(returnedData.lstPaymentPlan);
            self.lstPaymentTypeMain(returnedData.lstPaymentType);
            self.PaymentPlanSelectedMain(0);
            self.PaymentTypeSelectedMain(0);
            self.IsPaymentRequired(returnedData.IsPaymentRequired);
            self.AdminURL(returnedData.AdminURL);
            self.OpenSearchDialog(false);
            self.CloseSearchDialog(true);
            
        });
        // alert('Add Students Clicked');
    };

    self.CloseSaveStatusDialog = function () {
        self.IsSaveStatusDialogOpen(false);
    };


    self.Save = function (dataItem) {

        if (self.errors().length > 0) {
            self.errors.showAllMessages();
            return;
        }

        if (!self.IsPaymentRequired()) {
            self.ConfirmationViewModel.Show("There are already unused credits existing for this student, are you sure you would like to process this invoice?", function () {
                self.ValidateAdminPasswordViewModel.Show(self.AdminURL(), self.ValidateAdminPassword);
            });
        } else {
            self.ValidateAdminPasswordViewModel.Show(self.AdminURL(), self.ValidateAdminPassword);
        }
    };

    self.errors = ko.validation.group(self, { deep: true, observable: false });

    $(".date").datepicker({ dateFormat: 'dd/mm/yy', changeMonth: true, changeYear: true, yearRange: '1940:2022', beforeShowDay: self.disableSpecificWeekDays });

};