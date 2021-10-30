
function SearchInvoiceCriteria(DateGeneratedFrom, DateGeneratedTo, StudentNo, FirstName, LastName) {

    var self = this;

    self.StudentNo = ko.observable(StudentNo);
    self.FirstName = ko.observable(FirstName);
    self.LastName = ko.observable(LastName);
    self.DateGeneratedFrom = ko.observable(DateGeneratedFrom);
    self.DateGeneratedTo = ko.observable(DateGeneratedTo);
    self.PageSize = ko.observable(10);
    self.PageIndex = ko.observable(0);
    self.InvoiceStatus = ko.observable("");
}


function RecieptViewModel(recieptViewModel) {
    var self = this;
    self.NumberOfSessionsPaidFor = ko.observable(recieptViewModel.NumberOfSessionsPaidFor);
    self.AmountPaid = ko.observable(recieptViewModel.AmountPaid);
    self.PaymentDate = ko.observable(recieptViewModel.PaymentDate);
    self.PaymentType = ko.observable(recieptViewModel.PaymentType);
    self.ChequeNo = ko.observable(recieptViewModel.ChequeNo);

    self.ShowChequeNo = function () {
        return self.PaymentType() == 'Cheque';
    };


}

function AttendeesObject(attendeesObjModel) {

    this.StudentNo = ko.observable(attendeesObjModel.StudentNo);
    this.Status = ko.observable(attendeesObjModel.Status);
    this.Session = ko.observable(attendeesObjModel.Session);
    this.SessionAttendanceId = ko.observable(attendeesObjModel.SessionAttendanceId);
    this.listAttendanceStatus = ko.observable(attendeesObjModel.listAttendanceStatus);
    this.RemainingCredits = ko.observable(attendeesObjModel.RemainingCredits);
    this.IsDailyInvoiceData = ko.observable(attendeesObjModel.IsDailyInvoiceData);
    this.ChargedTo = ko.observable(attendeesObjModel.ChargedTo);
    this.InvoiceId = ko.observable(attendeesObjModel.InvoiceId);
    this.SelectedValue = ko.observable(attendeesObjModel.SelectedValue);
    this.listFutureInvoices = ko.observable(attendeesObjModel.listFutureInvoices);
    this.lstPaymentPlan = ko.observable(attendeesObjModel.lstPaymentPlan);
    this.lstPaymentType = ko.observable(attendeesObjModel.lstPaymentType);
    this.FutureInvoiceSelect = ko.observable(attendeesObjModel.FutureInvoiceSelect);
    this.PaymentPlanSelected = ko.observable(attendeesObjModel.PaymentPlanSelected);
    this.PaymentTypeSelected = ko.observable(attendeesObjModel.PaymentTypeSelected);
    this.ChequeNo = ko.observable(attendeesObjModel.ChequeNo);
    this.PaymentDate = ko.observable(attendeesObjModel.PaymentDate);
    this.PaymentDateInStr = ko.observable(attendeesObjModel.PaymentDateInStr);
    this.IsNewInvoiceEnabledRow = ko.observable(attendeesObjModel.IsNewInvoiceEnabledRow);
    this.IsFutureInvoiceAvailable = ko.observable(attendeesObjModel.IsFutureInvoiceAvailable);
}

function InvalidateInvoiceViewModel(invalidateinvoiceViewModel) {

    var self = this;

    //self.listStudentAttendanceDetails = ko.observableArray(invalidateinvoiceViewModel.listStudentAttendanceDetails);
    if (jQuery.isEmptyObject(invalidateinvoiceViewModel)) {

        self.listStudentAttendanceDetails = ko.observableArray([]);
    } else {

        self.listStudentAttendanceDetails = ko.observableArray(invalidateinvoiceViewModel.listStudentAttendanceDetails.map(function (objectdata) {

            return new AttendeesObject(objectdata);
        }));
    }
    self.Id = ko.observable(invalidateinvoiceViewModel.Id);
    self.SaveUrl = ko.observable(invalidateinvoiceViewModel.SaveUrl);
    self.CreditNote = ko.observable(invalidateinvoiceViewModel.CreditNote).extend({ required: true });;
    self.PaymentPlanSelectedInv = ko.observable(invalidateinvoiceViewModel.PaymentPlanSelectedInv);
    self.PaymentTypeSelectedInv = ko.observable(invalidateinvoiceViewModel.PaymentTypeSelectedInv);
    self.ChequeNoInv = ko.observable(invalidateinvoiceViewModel.ChequeNoInv);
    self.PaymentDateInv = ko.observable(invalidateinvoiceViewModel.PaymentDateInv);
    self.SessionAttendanceId = ko.observable(invalidateinvoiceViewModel.SessionAttendanceId);
    self.IsInvalidateInvoiceEnabled = ko.observable(invalidateinvoiceViewModel.IsInvalidateInvoiceEnabled);
    self.IsCreateNewInvoiceEnableInvalidate = ko.observable(invalidateinvoiceViewModel.IsCreateNewInvoiceEnableInvalidate);
    self.IsFutureInvoiceAvailable = ko.observable(invalidateinvoiceViewModel.IsFutureInvoiceAvailable);

    self.errors = ko.validation.group(self, { deep: false, observable: false });

    self.IsInvalidatable = function () {
        return self.IsInvalidateInvoiceEnabled() && self.errors().length <1;
    }

    
    self.CreditNote.subscribe(function (creditnotetext) {
        if (creditnotetext.length > 0) {
            $('#CreditNotes').removeClass('input-validation-error');
            $('#CreditNotes').addClass('input-validation-valid-TextArea-Creditnote');
        } else {
            $('#CreditNotes').addClass('input-validation-error');
            $('#CreditNotes').removeClass('input-validation-valid-TextArea-Creditnote');
        }
    });

    

}

function CreateInvoiceInvalidateViewModel(createinvoiceinvalidateViewModel) {

    var self = this;

    self.lstPaymentPlan = ko.observable(createinvoiceinvalidateViewModel.lstPaymentPlan);
    self.lstPaymentType = ko.observable(createinvoiceinvalidateViewModel.lstPaymentType);
    self.FutureInvoiceSelect = ko.observable(createinvoiceinvalidateViewModel.FutureInvoiceSelect);
    self.PaymentPlanSelected = ko.observable(createinvoiceinvalidateViewModel.PaymentPlanSelected);
    self.PaymentTypeSelected = ko.observable(createinvoiceinvalidateViewModel.PaymentTypeSelected);
    self.ChequeNo = ko.observable(createinvoiceinvalidateViewModel.ChequeNo);
    self.PaymentDate = ko.observable(createinvoiceinvalidateViewModel.PaymentDate);
    

    


}

function InvoiceViewModel(invoiceViewModel) {

    var self = this;
    var reciept = invoiceViewModel.Reciept == undefined ? {} : invoiceViewModel.Reciept;

    self.Id = ko.observable(invoiceViewModel.Id);
    self.InvoiceNo = ko.observable(invoiceViewModel.InvoiceNo);
    self.DateOfGeneration = ko.observable(invoiceViewModel.DateOfGeneration);
    self.StudentNo = ko.observable(invoiceViewModel.StudentNo);
    self.Name = ko.observable(invoiceViewModel.Name);
    self.Total = ko.observable(invoiceViewModel.Total);
    self.TotalAfterDiscount = ko.observable(invoiceViewModel.TotalAfterDiscount);
    self.VATAmount = ko.observable(invoiceViewModel.VATAmount);
    self.VATLabel = ko.observable(invoiceViewModel.VATLabel);
    self.InvoiceType = ko.observable(invoiceViewModel.InvoiceType);
    self.SessionCredits = ko.observable(invoiceViewModel.SessionCredits);
    self.Reciept = ko.observable(new RecieptViewModel(reciept));
    self.Discount = ko.observable(invoiceViewModel.Discount);

}

function SearchViewModel(searchViewModel) {

    var self = this;

   self.ValidateAdminPasswordViewModel = new ValidateAdminPasswordViewModel({});

    
    self.Data = ko.observable(searchViewModel.Data);
    self.DoSearchUrl = searchViewModel.DoSearchUrl;
    self.SelectedInvoice = ko.observable(new InvoiceViewModel({}));

    self.lstPaymentPlanMain = ko.observableArray();
    self.lstPaymentTypeMain = ko.observableArray();
    var customcheck = function (val) {
        return val > 0;
    }

    var Checkvalue = function () {
        return self.PaymentTypeSelectedMain() > 0
    }

    self.PaymentPlanSelectedMain = ko.observable();
    self.PaymentTypeSelectedMain = ko.observable().extend({
        required: true,
        validation: {
            validator: customcheck

        }
    });
    self.ChequeNoMain = ko.observable('').extend({
        required: true,
        validation: {
            validator: Checkvalue

        }
    });

    

    self.PaymentDateMain = ko.observable().extend({ required: true });
    self.SessionAttIdTemp = ko.observable();
    //self.IsPaymentDone = ko.observable(searchViewModel.Data.IsPaymentDone);
    self.SelectedRow = ko.observable();
    self.AdminURL = ko.observable();
    self.AdminURLMakePayment = ko.observable();
    self.currentInvoiceId = ko.observable();



    self.showUpdatePaymentModal = ko.observable(false);
    self.isOpen = ko.observable(false);
    self.makePaymentModal = ko.observable(false);
    //self.IsSearchDialogOpen = ko.observable(false);
    self.isInvalidateInvoice = ko.observable(false);
    self.InvalidateInvoice = ko.observable(new InvalidateInvoiceViewModel({}));
    self.newinvoiceinvalidate = ko.observable(false);
    self.listStudentAttendanceDetails = ko.observableArray([]);
    self.IsCreditNoteValidationDialogOpen = ko.observable(false);

    self.IsCloseCreditNoteValidation = function () {
        self.IsCreditNoteValidationDialogOpen(false);
    }

    self.closeinvalidateinvoice = function () {
        self.isInvalidateInvoice(false);
    }

    self.IsCreateNewInvoiceInInvalidateEnabled = ko.observable(false);


    //self.errors = ko.validation.group(self, { deep: false, observable: false });

    self.IsInvalidatableNewInvoice = function () {
        return self.IsCreateNewInvoiceInInvalidateEnabled();
    }

    //FutureInvoiceValueChanged
    self.FutureInvoiceValueChanged = function (obj, event) {

        var model = ko.mapping.toJSON(self.InvalidateInvoice());
        $.ajax(
            {
                type: 'POST',
                url: 'Payment/SaveNewInvoice',
                //data: {
                //    SessionAttendanceId: ko.toJS(self.SessionAttIdTemp()),
                //    PaymentType: ko.toJS(self.PaymentTypeSelectedMain()),
                //    PaymentPlan: ko.toJS(self.PaymentPlanSelectedMain()),
                //    ChequeNo: ko.toJS(self.ChequeNoMain()),
                //    PaymentDate: ko.toJS(self.PaymentDateMain()),
                //    invalidateInvoiceViewModel: model
                //},
                //data: JSON.stringify(model),
                contentType: 'application/json; charset=utf-8',
                data: model,
                dataType: 'json',
                success: function (data) {
                    self.newinvoiceinvalidate(false);
                    ////self.isInvalidateInvoice(false);
                    self.InvalidateInvoice(new InvalidateInvoiceViewModel({}));
                    self.InvalidateInvoice(new InvalidateInvoiceViewModel(data));

                    //self.isInvalidateInvoice(true);
                },
                error: function (req, status, err) {
                    console.log('something went wrong', status, err);
                    //alert('something went wrong' + status + err);
                }

            });
    }
    

    self.StatusValueChanged = function (obj, event) {
        
        var model = ko.mapping.toJSON(self.InvalidateInvoice());
        $.ajax(
            {
                type: 'POST',
                url: 'Payment/SaveNewInvoice',
                //data: {
                //    SessionAttendanceId: ko.toJS(self.SessionAttIdTemp()),
                //    PaymentType: ko.toJS(self.PaymentTypeSelectedMain()),
                //    PaymentPlan: ko.toJS(self.PaymentPlanSelectedMain()),
                //    ChequeNo: ko.toJS(self.ChequeNoMain()),
                //    PaymentDate: ko.toJS(self.PaymentDateMain()),
                //    invalidateInvoiceViewModel: model
                //},
                //data: JSON.stringify(model),
                contentType: 'application/json',
                data: model,
                dataType: 'json',
                success: function (data) {
                    self.newinvoiceinvalidate(false);
                    ////self.isInvalidateInvoice(false);
                    self.InvalidateInvoice(new InvalidateInvoiceViewModel({}));
                    self.InvalidateInvoice(new InvalidateInvoiceViewModel(data));

                    //self.isInvalidateInvoice(true);
                },
                error: function (req, status, err) {
                    console.log('something went wrong', status, err);
                    //alert('something went wrong' + status + err);
                }

            });
    }

    

    self.ExportExcel = function () {
        location.href = "/Payment/ExportToExcel?DateFrom=" + ko.toJS(self.SearchCriteria.DateGeneratedFrom) + "&DateTo=" + ko.toJS(self.SearchCriteria.DateGeneratedTo)
            + "&StudentNo=" + ko.toJS(self.SearchCriteria.StudentNo) + "&FirstName=" + ko.toJS(self.SearchCriteria.FirstName) + "&LastName=" + ko.toJS(self.SearchCriteria.LastName)
            + "&InvoiceStatus=" + ko.toJS(self.SearchCriteria.InvoiceStatus);
        //SearchCriteria
        //DateFrom: ko.toJS(self.SearchCriteria.GeneratedFrom),
        //    DateTo: ko.toJS(self.SearchCriteria.GeneratedTo)
        
        
        
    }

    self.StudentSearchViewModel = ko.observable(new StudentSearchViewModel({}));

    //self.OpenSearchDialog = function () {
    //    self.StudentSearchViewModel.SelectedStudents.removeAll();
    //    self.StudentSearchViewModel.Students.removeAll();
    //    this.IsSearchDialogOpen(true);
    //};

    self.makePayment = function (dataItem) {
        self.SelectedRow(dataItem);
        self.ValidateAdminPasswordViewModel.Show(dataItem.VerifyAdminPasswordUrl, self.loadMakePaymentDialog)        
    };


    self.loadMakePaymentDialog = function () {
        $.post(self.SelectedRow().MakePaymentDialogDataUrl, {}, function (returnedData) {

            self.InvalidateInvoice(new InvalidateInvoiceViewModel(returnedData));
            self.lstPaymentPlanMain(self.InvalidateInvoice().listStudentAttendanceDetails()[0].lstPaymentPlan());
            self.lstPaymentTypeMain(self.InvalidateInvoice().listStudentAttendanceDetails()[0].lstPaymentType());
            self.currentInvoiceId(self.InvalidateInvoice().listStudentAttendanceDetails()[0].InvoiceId());
            self.PaymentPlanSelectedMain(0);
            self.PaymentTypeSelectedMain(0);
            self.makePaymentModal(true);
        });
    }   

    self.makePaymentModalClose = function () {
        self.makePaymentModal(false);
    }

    self.ChequeNoMain.subscribe(function (valuedata) {
        var valuedata = self.PaymentTypeSelectedMain();
        if (valuedata > 0 && valuedata != 3005) {
            $('#newinvoicepaymentDate').removeClass('input-validation-valid');
            $('#newinvoicepaymentDate').addClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-error');
        }

        if (valuedata > 0 && valuedata == 3002 && self.PaymentPlanSelectedMain() > 0 && self.ChequeNoMain().length > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
        } else if (valuedata > 0 && valuedata == 3005) {
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
            self.ChequeNoMain('');
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
        } else if (valuedata > 0 && valuedata != 3002 && self.PaymentPlanSelectedMain() > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
            self.ChequeNoMain('');
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
        } else {
            self.IsCreateNewInvoiceInInvalidateEnabled(false);
        }
    });

    self.PaymentTypeSelectedMain.subscribe(function (valuedatas) {

        var valuedata = self.PaymentTypeSelectedMain();
        if (valuedata > 0 && valuedata != 3005) {
            $('#newinvoicepaymentDate').removeClass('input-validation-valid');
            $('#newinvoicepaymentDate').addClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-error');
        }

        if (valuedata > 0 && valuedata == 3002 && self.PaymentPlanSelectedMain() > 0 && self.ChequeNoMain().length > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
        } else if (valuedata > 0 && valuedata == 3005) {
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
            self.ChequeNoMain('');
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
        } else if (valuedata > 0 && valuedata != 3002 && self.PaymentPlanSelectedMain() > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
            self.ChequeNoMain('');
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
        } else {
            self.IsCreateNewInvoiceInInvalidateEnabled(false);
        }
    });

    self.PaymentDateMain.subscribe(function (valuedata) {

        var valuedata = self.PaymentTypeSelectedMain();
        if (valuedata > 0 && valuedata != 3005) {
            $('#newinvoicepaymentDate').removeClass('input-validation-valid');
            $('#newinvoicepaymentDate').addClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-error');
        }

        if (valuedata > 0 && valuedata == 3002 && self.PaymentPlanSelectedMain() > 0 && self.ChequeNoMain().length > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
        } else if (valuedata > 0 && valuedata == 3005) {
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
            self.ChequeNoMain('');
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
        } else if (valuedata > 0 && valuedata != 3002 && self.PaymentPlanSelectedMain() > 0 && ko.toJS(self.PaymentDateMain()) !== "" && ko.toJS(self.PaymentDateMain()) !== undefined) {
            $('#newinvoicepaymentDate').addClass('input-validation-valid');
            $('#newinvoicepaymentDate').removeClass('input-validation-error');
            $('#newinvoicepaymentplaninvalidate').addClass('input-validation-valid');
            $('#newinvoicepaymentplaninvalidate').removeClass('input-validation-error');
            self.ChequeNoMain('');
            self.IsCreateNewInvoiceInInvalidateEnabled(true);
        } else {
            self.IsCreateNewInvoiceInInvalidateEnabled(false);
        }
    });

    self.makepaymentsave = function (dataItem) {
        if (ko.toJS(self.PaymentTypeSelectedMain()) === 0 || ko.toJS(self.PaymentDateMain()) === undefined || ko.toJS(self.PaymentDateMain()) === "") {
            self.IsCreditNoteValidationDialogOpen(true);
            return;
        }
        $.ajax(
            {
                type: 'POST',
                url: 'Payment/GeneratePayment',
                data: {
                    InvoiceId: ko.toJS(self.currentInvoiceId()),
                    PaymentType: ko.toJS(self.PaymentTypeSelectedMain()),
                    PaymentPlan: ko.toJS(self.PaymentPlanSelectedMain()),
                    ChequeNo: ko.toJS(self.ChequeNoMain()),
                    PaymentDate: ko.toJS(self.PaymentDateMain())
                },
                success: function (data) {
                    location.reload(true);
                }

            });
    };

    self.CloseSearchDialog = function () {
        this.IsSearchDialogOpen(false);
    };


    self.filterInvalidate = ko.observable();

    self.filteredInvalidateViewModel = ko.computed(function () {
        if (!self.filterInvalidate()) {
            return self.InvalidateInvoice().listStudentAttendanceDetails;
        } else {
            return ko.utils.arrayFilter(self.listStudentAttendanceDetails(), function (prod) {
                return prod.SessionAttendanceId == self.filterInvalidate();
            });
        }
    });



    self.filterInvoice = function (dataItem) {
        
        self.SessionAttIdTemp(dataItem);
        //self.filterInvalidate(dataItem);
        self.lstPaymentPlanMain(self.InvalidateInvoice().listStudentAttendanceDetails()[0].lstPaymentPlan());
        self.lstPaymentTypeMain(self.InvalidateInvoice().listStudentAttendanceDetails()[0].lstPaymentType());
        self.PaymentPlanSelectedMain(0);
        self.PaymentTypeSelectedMain(0);
        self.PaymentDateMain("");
        
        self.newinvoiceinvalidate(true);
    }

    self.newinvoiceforinvalidateclose = function () {
        self.newinvoiceinvalidate(false);
    }

    self.NewInvoice = function (dataItem) {
        self.newinvoiceinvalidate(true);
    }

    self.close = function () {
        self.isOpen(false);
    };

    self.savenewinvoice = function (dataItem) {
        
        self.InvalidateInvoice().PaymentPlanSelectedInv(self.PaymentPlanSelectedMain());
        self.InvalidateInvoice().PaymentTypeSelectedInv(self.PaymentTypeSelectedMain());
        self.InvalidateInvoice().ChequeNoInv(self.ChequeNoMain());
        self.InvalidateInvoice().PaymentDateInv(self.PaymentDateMain());
        self.InvalidateInvoice().SessionAttendanceId(self.SessionAttIdTemp());
        var currdata = self.PaymentDateMain();
        //if (self.PaymentPlanSelectedMain() == 0 || self.PaymentTypeSelectedMain() == 0 || self.PaymentDateMain() == "" || typeof currdata === "undefined") {
        if (((self.PaymentTypeSelectedMain() > 0 && self.PaymentTypeSelectedMain() != 3005) && 
            (self.PaymentPlanSelectedMain() == 0 || self.PaymentDateMain() == "" || typeof currdata === "undefined")) || self.PaymentTypeSelectedMain() == 0
        ) {
            self.IsCreditNoteValidationDialogOpen(true);
        } else {


            var model = ko.mapping.toJSON(self.InvalidateInvoice());
            $.ajax(
                {
                    type: 'POST',
                    url: 'Payment/SaveNewInvoice',
                    //data: {
                    //    SessionAttendanceId: ko.toJS(self.SessionAttIdTemp()),
                    //    PaymentType: ko.toJS(self.PaymentTypeSelectedMain()),
                    //    PaymentPlan: ko.toJS(self.PaymentPlanSelectedMain()),
                    //    ChequeNo: ko.toJS(self.ChequeNoMain()),
                    //    PaymentDate: ko.toJS(self.PaymentDateMain()),
                    //    invalidateInvoiceViewModel: model
                    //},
                    //data: JSON.stringify(model),
                    contentType: 'application/json; charset=utf-8',
                    data: model,
                    dataType: 'json',
                    success: function (data) {
                        self.newinvoiceinvalidate(false);
                        ////self.isInvalidateInvoice(false);
                        self.InvalidateInvoice(new InvalidateInvoiceViewModel({}));
                        self.InvalidateInvoice(new InvalidateInvoiceViewModel(data));

                        //self.isInvalidateInvoice(true);
                    },
                    error: function (req, status, err) {
                        console.log('something went wrong', status, err);
                        //alert('something went wrong' + status + err); 
                    }

                });
        }

    }

    self.view = function (dataItem) {
        $.post(dataItem.ViewInvoiceUrl, {}, function (returnedData) {
            self.SelectedInvoice(new InvoiceViewModel(returnedData));
            self.isOpen(true);
        });
    };


    self.updateInvoicePaymentDetails = function () {

        $.post(self.SelectedRow().UpdatePaymentUrl, { invoiceId: ko.toJS(self.SelectedInvoice().Id), paymentDate: ko.toJS(self.SelectedInvoice().Reciept().PaymentDate) }, function (returnedData) {
            if (returnedData == true) {
                self.showUpdatePaymentModal(false);
            }
        });            
    }

    self.loadEditInvoiceDialog = function () {
        //alert("loading data");

        $.post(self.SelectedRow().ViewInvoiceUrl, {}, function (returnedData) {
            //format date          
            self.SelectedInvoice(new InvoiceViewModel(returnedData));
            self.showUpdatePaymentModal(true);
        });
    }

    self.hideEditInvoiceDialog = function () {
        self.showUpdatePaymentModal(false);

    }

    self.edit = function (dataItem) {
        self.SelectedRow(dataItem);
        self.ValidateAdminPasswordViewModel.Show(dataItem.VerifyAdminPasswordUrl, self.loadEditInvoiceDialog);
    }

    self.invalidate = function (dataItem) {
        self.SelectedRow(dataItem);
        self.ValidateAdminPasswordViewModel.Show(dataItem.VerifyAdminPasswordUrl, self.loadInvalidateDialog);
    };

    self.loadInvalidateDialog = function () {
        $.post(self.SelectedRow().MakePaymentDialogDataUrl, {}, function (returnedData) {
            self.InvalidateInvoice(new InvalidateInvoiceViewModel(returnedData));
            self.isInvalidateInvoice(true);
        });
    };

    self.save = function () {

        if (self.InvalidateInvoice().errors().length > 0) {
            self.InvalidateInvoice().errors.showAllMessages();
            return;
        }


        var model = ko.mapping.toJSON(self.InvalidateInvoice());

        $.ajax({
            type: 'POST',
            cache: false,
            url: 'Payment/SaveInvalidateInvoice',
            contentType: 'application/json; charset=utf-8',
            data: model,
            processData: false,
            datatype: 'json',
            success: function (res) {
                //$('#invalidateinvoicepartial').modal('hide');
                location.reload(true);
            }
        });

    };

    self.download = function (dataItem) {
        location.href = dataItem.DownloadInvoiceUrl;
    };

    self.delete = function (dataItem) {
        if (confirm("Are you sure you would like to delete this student")) {
            location.href = dataItem.DeleteUrl;
        }
    };

    self.AddRegister = function () {
        location.href = self.AddItemUrl;
    };

    self.Search = function () {
        var criteria = ko.toJS(self.SearchCriteria);
        $.post(self.DoSearchUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Data(returnedData.Data);
            self.Pager(new Pager(criteria, returnedData.MaxPageIndex, self.MoveToPage));
        });

    };

    self.MoveToPage = function () {
        var criteria = self.Pager().SearchCriteria;
        $.post(self.DoSearchUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Data(returnedData.Data);
        });
    };

    self.SearchCriteria = new SearchInvoiceCriteria(
        searchViewModel.DateGeneratedFrom,
        searchViewModel.DateGeneratedTo,
        searchViewModel.StudentNo,
        searchViewModel.FirstName,
        searchViewModel.LastName
    );

    self.Pager = ko.observable(new Pager(ko.toJS(self.SearchCriteria), searchViewModel.MaxPageIndex, self.MoveToPage));

    $(".date").datepicker({ dateFormat: 'dd/mm/yy', changeMonth: true, changeYear: true, yearRange: '1940:2022' });

}


function SearchCriteria() {
    var self = this;

    self.FirstName = ko.observable("");
    self.LastName = ko.observable("");
    self.StudentNo = ko.observable("");
    self.Curriculum = ko.observable("");
    self.ActiveOnly = ko.observable(true);
    self.PageSize = ko.observable(10);
    self.PageIndex = ko.observable(0);

}

function StudentSearchViewModel(studentSearchViewModel, callback) {
    var self = this;
    self.Students = ko.observableArray(studentSearchViewModel.Students);
    self.SearchStudentsUrl = studentSearchViewModel.SearchStudentsUrl;
    self.AddStudentUrl = studentSearchViewModel.AddStudentUrl;
    self.CanAddNewStudent = studentSearchViewModel.CanAddNewStudent;
    self.AreStudentsSelectable = studentSearchViewModel.AreStudentsSelectable;

    self.SelectedStudents = ko.observableArray([]);
    self.CallBack = callback;

    self.ActionOnSelectedStudents = function () {
        if (self.SelectedStudents().length < 1) return;
        if (self.CallBack != null && self.CallBack != undefined)
            self.CallBack(self.SelectedStudents());
        // alert('Add Students Clicked');
    };


    // self.SelectedStudents.subscribe(function(newValue) {
    //   alert(newValue);
    //        var newSelectedUserNames = newValue;
    //        var newSelectedUsers = [];
    //        ko.utils.arrayForEach(newSelectedUserNames, function(userName) {
    //            var selectedUser = ko.utils.arrayFirst(self.AllUsers(), function(user) {
    //                return (user.userName() === userName);
    //            });
    //            newSelectedUsers.push(selectedUser);
    //        });
    //        self.SelectedUsers(newSelectedUsers);
    //});

    self.edit = function (student) {
        location.href = student.EditUrl;
        //alert("called");
    };

    self.delete = function (student) {
        if (confirm("Are you sure you would like to delete this student")) {
            location.href = student.DeleteUrl;
        }
    };

    self.AddStudent = function () {
        location.href = self.AddStudentUrl;
    };

    self.SearchStudents = function () {
        var criteria = ko.toJS(self.SearchCriteria);
        $.post(self.SearchStudentsUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Students(returnedData.Students);
            self.Pager(new Pager(criteria, returnedData.MaxPageIndex, self.MoveToPage));
        });

    };

    self.MoveToPage = function () {
        var criteria = self.Pager().SearchCriteria;
        $.post(self.SearchStudentsUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Students(returnedData.Students);
        });
    };

    self.SearchCriteria = new SearchCriteria();
 
    self.Pager = ko.observable(new Pager(ko.toJS(self.SearchCriteria), studentSearchViewModel.MaxPageIndex, self.MoveToPage));

}