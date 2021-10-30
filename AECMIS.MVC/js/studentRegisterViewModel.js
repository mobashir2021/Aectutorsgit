 
ko.bindingHandlers.parentvalElement = {
            update: function(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var t = valueAccessor();
                var valueIsValid = valueAccessor().isValid();//&& valueAccessor().outstandingPaymentsIsValid(); //&& viewModel.isAnyMessageShown()
                if (!valueIsValid ) {
                    $(element).addClass("rowError");
                } else {
                    $(element).removeClass("rowError");
                }
            }
        };


var studentAttendance = function(studentAttendanceViewModel) {

    var self = this, data;

    data = $.extend({ Subjects: [], PaymentPlans: [], SessionAttendanceViewModel: { }, PaymentRequiredPlanId: "", PaymentRequired: "", FirstName: "", LastName: "" },
        studentAttendanceViewModel);
    self.SessionAttendanceViewModel = ko.validatedObservable(new subjectAttendance(data.SessionAttendanceViewModel));
    self.FirstName = data.FirstName;
    self.LastName = data.LastName;
    self.StudentNo = data.StudentNo;
    self.Subjects = data.Subjects;
    self.StudentId = data.StudentId;

    self.Name = ko.computed(function() {
        return self.FirstName + " " + self.LastName;
    });

};


var PaymentViewModel = function (paymentViewModel, subjectViewModel) {
    var self = this, data;

    data = $.extend({ PaymentPlans: [], PaymentAmount: {}, PaymentRequired: "", PaymentType: "", ChequeNo: "", InvoiceId: "", IsFutureInvoice: false, PaymentDate: "" }, paymentViewModel);
    data.SubjectViewModel = subjectViewModel;


    ko.mapping.fromJS(data, {

        IsFutureInvoice: {
            create: function (options) {
                return options.data;
            }
        },
        InvoiceId: {
            create: function (options) {
                return options.data;
            }
        },
        //payment amount is required when 
        PaymentAmount: {
            create: function (options) {
                return ko.observable(options.data)
                         .extend({
                             required: {
                                 message: "Payment amount is required",
                                 onlyIf: function () {
                                     var s = (
                                         self.SubjectViewModel.Command() == 1001 && self.SubjectViewModel.Status() == 1001 && self.SubjectViewModel.PaymentRequired() &&
                                         self.PaymentType() != 3005 && self.IsFutureInvoice);
                                     var t = (self.IsFutureInvoice == false && self.PaymentType() != 3005 && self.PaymentType() != undefined);
                                     return (s || t);

                                 }
                             }//|| (self.IsFutureInvoice == false && self.PaymentType() != 3005)

                         });
            }
        },
        //payment type is required when it is future invoice 
        PaymentType: {
            create: function (options) {
                return ko.observable(options.data)
                         .extend({
                             required: {
                                 message: "Payment Type is required.",
                                 onlyIf: function () {
                                     return (self.SubjectViewModel.Command() == 1001 && self.SubjectViewModel.Status() == 1001
                                         && self.SubjectViewModel.PaymentRequired() && self.IsFutureInvoice);
                                 }
                             }
                         });
            }
        },
        ChequeNo: {
            create: function (options) {
                return ko.observable(options.data)
                         .extend({
                             required: {
                                 message: "ChequeNo is required when payment type is cheque",
                                 onlyIf: function () {
                                     var s = (self.PaymentType() == 3002 && self.SubjectViewModel.Command() == 1001 &&
                                         self.SubjectViewModel.Status() == 1001 && self.SubjectViewModel.PaymentRequired() && self.IsFutureInvoice);
                                     var t = (self.IsFutureInvoice == false && self.PaymentType() == 3002);

                                     return (s || t);
                                 }
                             }
                         });
                //.extend({ digit: true});
            }
        },
        PaymentDate: {
            create: function (options) {
                return ko.observable(options.data)
                         .extend({
                             required: {
                                 message: "Payment Date is required.",
                                 onlyIf: function () {
                                     var s = (
                                         self.SubjectViewModel.Command() == 1001 && self.SubjectViewModel.Status() == 1001 && self.SubjectViewModel.PaymentRequired() &&
                                             self.PaymentType() != 3005 && self.IsFutureInvoice);
                                     var t = (self.IsFutureInvoice == false && self.PaymentType() != 3005 && self.PaymentType() != undefined);
                                     return (s || t);
                                 }
                             }
                         });
            }
        }
    }, self);
};

var subjectAttendance = function (subjectAttendanceViewModel) {
    var self = this, data;

    data = $.extend({ MainViewModel: {}, AttendanceId: "", StudentId: "", Status: "", SubjectId: "", TeacherId: "", PaymentRequired: "", OutstandingPayments: [], FutureInvoicePayment: {}, Notes: "" }, subjectAttendanceViewModel);
    //data.MainViewModel = viewModel;

    //self.MainViewModel = viewModel;   
    self.Command = ko.observable("1000");
    self.PaymentRequired = data.PaymentRequired;
    self.AttendanceId = data.AttendanceId;
    self.StudentId = data.StudentId;
    self.Status = ko.observable(data.Status).extend({
        required: {
            message: "Status is required",
            onlyIf: function () {
                return self.Command() == 1001;
            }
        }
    });

    self.SubjectId = ko.observable(data.SubjectId)
                     .extend({ required: { message: "Subject is required."} });

    self.TeacherId = ko.observable(data.TeacherId)
        .extend({
            required: {
                message: "Teacher is required.",
                onlyIf: function() {
                    return self.Command() == 1001;
                }
            }
        });
    self.FutureInvoicePayment = data.FutureInvoicePayment == null ? null : new PaymentViewModel(data.FutureInvoicePayment, self);

    self.OutstandingPayments = ko.observableArray([]); //ko.validatedObservable(new PaymentViewModel(data.OutstandingPayments, self));
    var s = ko.utils.arrayMap(data.OutstandingPayments, function (s) {
        return ko.validatedObservable(new PaymentViewModel(s, self));
    });
    self.OutstandingPayments.push.apply(self.OutstandingPayments, s);

    self.Notes = ko.observable(data.Notes);
    self.PreviousNotes = data.PreviousNotes;
};


 var saveStatus = function(saveStatus) {

     self.NavigationActions = saveStatus.actions;
     self.Message = ko.observable(saveStatus.message);
     self.Success = ko.observable(saveStatus.success);

 };

// var action = function(action) {
//     self.ActionText = action.Text;
//     self.ActionUrl = action.Url;
// };


 var SessionRegisterViewModel = function (registerViewModel, allSessions) {
     var self = this, data;

     data = $.extend({
         Command: 1000,
         SessionRegisterId: "",
         SessionId: "",
         Center: "",
         Date: "",
         SessionAttendees: [],
         Teachers: [],
         ExistingRegisters: [],
         GetRegisterTemplateUrl: "",
         GetStudentsStudyPlansUrl: "",
         isOpen: false,
         StudentSearchViewModel: {},
         SelectedSessionAttendance: {},
         IsSearchDialogOpen: false,
         IsPaymentsDialogOpen: false,
         IsNotesDialogOpen: false,
         SaveStatus: {},
         IsSaveStatusDialogOpen: false,
         AdminPassword: "",
         ValidateAdminPasswordViewModel: {}
     }, registerViewModel);


     //isOpen: ko.observable(false);
     self.OpenSearchDialog = function () {
         self.StudentSearchViewModel.SelectedStudents.removeAll();
         self.StudentSearchViewModel.Students.removeAll();
         this.IsSearchDialogOpen(true);
     };

     self.AllSessions = allSessions;

     self.CloseSearchDialog = function () {
         this.IsSearchDialogOpen(false);
     };


     self.ViewOutstandingPayments = function (selectedAttendance) {

         self.SelectedSessionAttendance(selectedAttendance);
         self.IsPaymentsDialogOpen(true);
     };

     self.ClosePaymentsDialog = function () {


         var invalidPayments = ko.utils.arrayFilter(self.SelectedSessionAttendance().OutstandingPayments(), function (s) {
             return s.isValid() == false;
         });

         if (invalidPayments.length < 1) {
             self.IsPaymentsDialogOpen(false);
             return true;
         } else {
             //ko.validation.group(self, { deep: true, live: true }).showAllMessages();
             return false;
         }
     };

     self.ViewNotes = function (selectedAttendance) {
         self.SelectedSessionAttendance(selectedAttendance);
         self.IsNotesDialogOpen(true);
     };

     self.CloseNotesDialog = function () {
         self.IsNotesDialogOpen(false);
     };


     self.CloseSaveStatusDialog = function () {
         self.IsSaveStatusDialogOpen(false);
     };

     self.AddStudentsToRegister = function (studentIds) {
         self.CloseSearchDialog();
         if (self.SessionId() < 1 || self.SessionId() == undefined) return;

         //remove students that are already on the register
         ko.utils.arrayFilter(self.SessionAttendees(), function (s) {
             var index = jQuery.inArray(s.StudentId.toString(), studentIds);
             if (index > -1)
                 studentIds.splice(index, 1);
         });

         if (studentIds.length < 1) return;

         ShowProgressBar($('body'), { bgColor: '#000', opacity: '0.3' });
         var serverData = JSON.stringify({ studentIds: studentIds, sessionId: self.SessionId() });


         jQuery.ajax({
             url: self.GetStudentsStudyPlansUrl,
             type: "POST",
             contentType: "application/json",
             dataType: "json",
             data: serverData,
             success: function (returnedData) {
                 self.AddSessionAttendees(returnedData);
             }
         });

         HideProgressBar($('body'));
     };

     ko.mapping.fromJS(data, {
         Center: {
             create: function (options) {
                 return ko.observable(options.data);
             }
         },
         SessionId: {
             create: function (options) {
                 return ko.observable(options.data);
             }
         },
         Date: {
             create: function (options) {
                 return ko.observable(options.data).extend({ required: { message: "Date is required."} });
             }
         },
         GetRegisterTemplateUrl: {
             create: function (options) {
                 return options.data;
             }
         },
         Command: {
             create: function (options) {
                 return ko.observable(options.data);
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
         AdminPassword: {
             create: function (options) {
                 return ko.observable();
             }
         },
         ValidateAdminPasswordViewModel: {
             create: function (options) {
                 return new ValidateAdminPasswordViewModel(options.data);
             }
         }
     
     }, self);

     self.Save = function () {
         self.UpdateCommandOnModel(1000);
         self.PostForm(1000);
     };

     self.DeleteSessionAttendee = function (sessionAttendee) {
         self.SessionAttendees.remove(sessionAttendee);
     };
     self.PostForm = function (command) {

         if (self.errors() < 1) {

             var s = ko.mapping.toJS(self, { ignore: ["ExistingRegisters", "AllSessions", "AllTeachers", "Centers", "AttendanceStatuses", "curriculumTypes"] });
             s.CommandType = command;
             ko.utils.arrayForEach(s.SessionAttendees, function (t) {
                 delete t.SessionAttendanceViewModel.MainViewModel;
                 if (t.SessionAttendanceViewModel.FutureInvoicePayment != undefined)
                     delete t.SessionAttendanceViewModel.FutureInvoicePayment.SubjectViewModel;
             });

             //console.log(t);
             ShowProgressBar($('body'), { bgColor: '#000', opacity: '0.3' });
             $.post(self.SaveRegisterUrl(), { model: ko.utils.stringifyJson(s), commandType: command }, function (returnedData) {

                 self.IsSaveStatusDialogOpen(true);
                 self.SaveStatus({ Message: returnedData.message, NavigationActions: returnedData.actions, Success: returnedData.success });
                 HideProgressBar($('body'));
             }, "json");

         } else {
             // ko.validation.group(self, { deep: true, live: true }).showAllMessages();
             //self.errors.showAllMessages(false);
         }
     };

     self.UpdateCommandOnModel = function (command) {
         ko.utils.arrayForEach(self.SessionAttendees(), function (s) {
             s.SessionAttendanceViewModel().Command(command);
         });
     };

     self.MinimiseModel = function () {
         ko.utils.arrayForEach(self.SessionAttendees, function (s) {
             delete s.SessionAttendanceViewModel.MainViewModel;
         });
     };

     self.ProcessRegister = function () {
         self.UpdateCommandOnModel(1001);
         if (self.errors().length > 0) {
             self.errors.showAllMessages();
             return;
         }
         self.ValidateAdminPasswordViewModel.Show(self.VerifyAdminPasswordUrl(), self.CompleteProcessRegister)
     };

     self.CompleteProcessRegister = function () {
         self.PostForm(1001);
     };

     self.ShowRegister = ko.computed(function () {
         return self.SessionAttendees().length > 0;
     });

     self.SessionsByCenter = ko.computed(function () {
         var center = self.Center();

         return ko.utils.arrayFilter(self.AllSessions, function (s) {
             return s.Location == center;
         });
     }, self);

     self.SelectedSession = function () {
         return ko.utils.arrayFilter(self.AllSessions, function (s) {
             return s.SessionId == self.SessionId();
         })[0];
     };

     self.SessionId.subscribe(function (sessionId) {

         if (!self.SessionRegisterId() > 0) {
             self.SessionAttendees([]);
             self.ExistingRegisters([]);
         }
         if (sessionId == undefined || self.SessionRegisterId > 0) return;
         var session = { SessionId: sessionId };

         ShowProgressBar($('body'), { bgColor: '#000', opacity: '0.3' });
         $.post(self.GetRegisterTemplateUrl, session, function (returnedData) {
             // This callback is executed if the post was successful    
             //self.SessionAttendees(returnedData.SessionAttendees);
             //self.RegisterVisible(true);

             self.AddSessionAttendees(returnedData.SessionAttendees);
             self.AddExistingRegisters(returnedData.ExistingRegisters);
             HideProgressBar($('body'));
         });


     });

     self.UpdatePaymentDates = function () {
         ko.utils.arrayForEach(self.SessionAttendees(), function (sa) {
             if (sa.SessionAttendanceViewModel().FutureInvoicePayment != null && sa.SessionAttendanceViewModel().FutureInvoicePayment.PaymentType != 3005 &&
                          sa.SessionAttendanceViewModel().Status() == 1001) {
                 sa.SessionAttendanceViewModel().FutureInvoicePayment.PaymentDate(self.Date());
             }
         });
     };


     self.AddExistingRegisters = function (existingRegisterDates) {
         ko.utils.arrayForEach(existingRegisterDates, function (s) {
             var date = new Date(parseInt(s.replace(/\/Date\((-?\d+)\)\//gi, "$1")));
             self.ExistingRegisters.push(date);
         });
     };

     self.AddSessionAttendees = function (sessionAttendees) {

         var newItems = ko.utils.arrayMap(sessionAttendees, function (s) {
             return new studentAttendance(s, self);
         });
         self.SessionAttendees.push.apply(self.SessionAttendees, newItems);

     };

     self.ExistingRegisterDateContains = function (date) {
         return ko.utils.arrayFilter(self.ExistingRegisters(), function (s) {
             return s.getDate() == date.getDate() && s.getMonth() == date.getMonth() && s.getYear() == date.getYear();
         });
     };
     self.disableSpecificWeekDays = function (date) {

         //if (self.ExistingRegisters.length == 0 && self.ExistingRegisters[0] == undefined) return [true];
         if (self.RegisterLocationDetailsAreReadOnly()) return true;

         if (self.ExistingRegisters() != null && self.ExistingRegisters.length >= 0 && self.ExistingRegisters[0] != undefined &&
             Object.prototype.toString.call(self.ExistingRegisters()[0]) != "[object Date]") return [false];
         var day = date.getDay();

         //date.
         var session = self.SelectedSession();
         if ((session != undefined && session.Day != day) || self.ExistingRegisterDateContains(date).length > 0) {
             return [false];
         }
         return [true];
     };

     self.errors = ko.validation.group(self, { deep: true, observable: false });

     //   $(".date").datepicker({ dateFormat: 'dd/mm/yy', changeMonth: true, changeYear: true, yearRange: '1940:2022',beforeShowDay:self.disableSpecificWeekDays });            

 };