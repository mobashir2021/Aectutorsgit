

function SearchCriteria(attendancesFrom, attendancesTo) {

    var self = this;

    self.StudentNo = ko.observable("");
    self.FirstName = ko.observable("");
    self.LastName = ko.observable("");
    self.DOB = ko.observable("");
    self.Curriculum = ko.observable("");
    self.Subject = ko.observable("");
    self.PageSize = ko.observable(10);
    self.PageIndex = ko.observable(0);
    self.SessionsFromDate = ko.observable(attendancesFrom);
    self.SessionsToDate = ko.observable(attendancesTo);

}

function ChangeAttendanceViewModel(changeattendanceViewModel) {

    var self = this;

    self.studentno = ko.observable(changeattendanceViewModel.StudentNo);
    self.Status = ko.observable(changeattendanceViewModel.Status);
    self.Session = ko.observable(changeattendanceViewModel.Session);
    self.SessionAttendanceId = ko.observable(changeattendanceViewModel.SessionAttendanceId);
    self.attendancestatus = ko.observable(changeattendanceViewModel.listAttendanceStatus);
    self.RemainingCredits = ko.observable(changeattendanceViewModel.RemainingCredits);
    self.RemainingCreditInStr = ko.observable(changeattendanceViewModel.RemainingCreditInStr);
    self.IsDailyInvoiceData = ko.observable(changeattendanceViewModel.IsDailyInvoiceData);
    self.ChargedTo = ko.observable(changeattendanceViewModel.ChargedTo);
    self.InvoiceId = ko.observable(changeattendanceViewModel.InvoiceId);
    self.IsNewInvoiceEnabled = ko.observable(changeattendanceViewModel.IsNewInvoiceEnabled);
    self.IsSaveEnabled = ko.observable(changeattendanceViewModel.IsSaveEnabled);
    self.FutureInvoiceSelect = ko.observable(changeattendanceViewModel.FutureInvoiceSelect);
    self.listFutureInvoices = ko.observable(changeattendanceViewModel.listFutureInvoices);
    self.IsFutureInvoiceAvailable = ko.observable(changeattendanceViewModel.IsFutureInvoiceAvailable);
    self.FutureInvoiceValueChanged = function (obj, event) {

        if (self.FutureInvoiceSelect() > 0) {
            $("#createnewinvoice").hide();
            $('#savenewinvoiceCA').show();
        } else {
            $("#createnewinvoice").show();
            $('#savenewinvoiceCA').hide();
        }
    }
    //self.IsNewInvoiceEnabled = ko.observable(changeattendanceViewModel.IsNewInvoiceEnabled);
    //self.IsSaveEnabled = ko.observable(changeattendanceViewModel.IsSaveEnabled);
    //self.selectedOptionId = ko.observable(self.attendancestatus[0].AttendanceId);
}

function tempViewModel() {
    attendancestatus = [
        { AttendanceStatus: 'Attended', AttendanceId: 1, disable: ko.observable(false) }
    ];
}

function StudentAttendanceViewModel(studentAttendanceViewModel, subjects) {
    var self = this;
    self.Attendances = ko.observableArray(studentAttendanceViewModel.Attendances);
    self.SearchAttendancesUrl = studentAttendanceViewModel.SearchAttendancesUrl;
    self.AllSubjects = subjects;
    self.SearchCriteria = new SearchCriteria(studentAttendanceViewModel.AttendancesFrom, studentAttendanceViewModel.AttendancesTo);

    //self.SelectedAttendance = ko.observable(new ChangeAttendanceViewModel({ "Status": "", "Session": "", "attendancestatus": tempViewModel(), "SessionAttendanceId" : "0" }));
    self.SelectedAttendance = ko.observable(new ChangeAttendanceViewModel({}));
    self.isOpen = ko.observable(false);
    self.newInvoiceDialogOpen = ko.observable(false);
    self.isOpenInvoice = ko.observable(false);
    self.PaymentDate = ko.observable(new Date());
    self.ChequeNo = ko.observable("");
    self.SelectedRow = ko.observable();
    self.AdminURL = ko.observable();
    self.ValidateAdminPasswordViewModel =  new ValidateAdminPasswordViewModel({});
    self.lstPaymentPlanMain = ko.observableArray([]);
    self.lstPaymentTypeMain = ko.observableArray([]);
    self.PaymentPlanSelectedMain = ko.observable();
    self.PaymentTypeSelectedMain = ko.observable();
    self.IsNewInvoiceEnabledTrue = ko.observable(false);
    self.IsValidationPaymentDialogOpen = ko.observable(false);
    self.showInvalidAdminPasswordMessage = ko.observable(false); 

    //self.PaymentTypeSelectedMain.subscribe(function (valuedata) {
    //    alert(valuedata);
    //    if (valuedata > 0 && valuedata != 3005) {
    //        $('#newinvoicepaymentDate').removeClass('input-validation-valid');
    //        $('#newinvoicepaymentDate').addClass('input-validation-error');
    //    }
    //    if (valuedata > 0 && valuedata == 3002 && self.ChequeNoMain().length < 1) {
    //        $('#newinvoicechequeNo').removeClass('input-validation-valid');
    //        $('#newinvoicechequeNo').addClass('input-validation-error');
    //    }

    //    if (valuedata > 0 && valuedata == 3002 && self.ChequeNoMain().length > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
    //        self.IsCreateNewInvoiceInInvalidateEnabled(true);
    //        $('#newinvoicepaymentDate').addClass('input-validation-valid');
    //        $('#newinvoicepaymentDate').removeClass('input-validation-error');
    //    } else if (valuedata > 0 && valuedata == 3005) {
    //        $('#newinvoicepaymentDate').removeClass('input-validation-error');
    //        $('#newinvoicepaymentDate').addClass('input-validation-valid');
    //        self.ChequeNoMain('');
    //        self.IsCreateNewInvoiceInInvalidateEnabled(true);
    //    } else if (valuedata > 0 && valuedata != 3002 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
    //        self.ChequeNoMain('');
    //        self.IsCreateNewInvoiceInInvalidateEnabled(true);
    //    } else {
    //        self.IsCreateNewInvoiceInInvalidateEnabled(false);
    //    }
    //});
    

    self.IsValidationClose = function () {
        self.IsValidationPaymentDialogOpen(false);
        $('#IsValidationPaymentDialogOpenId').modal('hide');
    }
    

    self.createnewinvoice = function () {
        self.newInvoiceDialogOpen(true);
    };

    self.closenewinvoice = function () {
        self.newInvoiceDialogOpen(false);
    }

    self.changeattendanceclose = function () {
        self.SelectedAttendance(new ChangeAttendanceViewModel({}));
        $('#newinvoicepaymentDate').val("");
        self.isOpen(false);
        
    };

    self.CancelInvalidateInvoiceRef = function () {
        self.SelectedAttendance(new ChangeAttendanceViewModel({}));
        $('#newinvoicepaymentDate').val("");
        $('#invalidateinvoice').modal('hide');
    }


    self.SearchAttendances = function () {
        var criteria = ko.toJS(self.SearchCriteria);
        $.post(self.SearchAttendancesUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Attendances(returnedData.Attendances);
            self.Pager(new Pager(criteria, returnedData.MaxPageIndex, self.MoveToPage));
        });

    };

    self.edit = function (dataItem) {
        
        self.SelectedRow(dataItem);
        self.ValidateAdminPasswordViewModel.Show(dataItem.VerifyAdminPasswordUrl, self.loadModifyAttendanceDialog);
    };
    self.loadModifyAttendanceDialog = function () {

        $.post(self.SelectedRow().EditUrlDetails, {}, function (returnedData1) {
            self.SelectedAttendance(new ChangeAttendanceViewModel(returnedData1));
            self.lstPaymentPlanMain(returnedData1.lstPaymentPlan);
            self.lstPaymentTypeMain(returnedData1.lstPaymentType);
            self.IsNewInvoiceEnabledTrue(returnedData1.IsNewInvoiceEnabled);
            self.PaymentPlanSelectedMain(0);
            self.PaymentTypeSelectedMain(0);
            self.isOpen(true);
        });
    };



    self.MoveToPage = function () {
        var criteria = self.Pager().SearchCriteria;
        $.post(self.SearchAttendancesUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Attendances(returnedData.Attendances);
        });
    };

    self.Subjects = ko.computed(function () {
        return ko.utils.arrayFilter(self.AllSubjects, function (s) {
            return self.SearchCriteria.Curriculum() == s.Level;
        });
    });


    self.Pager = ko.observable(new Pager(ko.toJS(self.SearchCriteria), studentAttendanceViewModel.MaxPageIndex, self.MoveToPage));

    $(".date").datepicker({ dateFormat: 'dd/mm/yy', changeMonth: true, changeYear: true, yearRange: '1940:2022' });
}