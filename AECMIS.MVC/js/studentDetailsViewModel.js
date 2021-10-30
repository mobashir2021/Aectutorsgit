ko.validation.rules['Required'] = {
    validator: function (val, params) {        
        return val == undefined || val == null ||val.trim() == "";
    },
    message: 'Contact Details Required',
};

ko.validation.rules['checked'] = {
    validator: function (value) {
      //console.log(value);
        if (!value) {
            return false;
        }
        return true;
    }
};
ko.validation.registerExtenders();


function ContactPerson(contact) {
    var self = this, data;

  data = $.extend({ContactName:"",Type:"",Title:"", ContactAddress: {AddressRequired:contact.IsPrimaryContact},
      ContactPhone: {}}, contact);

    //ko.mapping.fromJS(contact, { }, self);
    ko.mapping.fromJS(data, {
        ContactPhone: {
            create: function(options) {
                return new ContactPhone(options.data);
            }
        },
        ContactAddress: {
            create: function(options) {
                return new ContactAddress(options.data);
            }
        },
        ContactName: {
            create: function(options) {
                return ko.observable(options.data).extend({ required: { message: "Contact Name is required." }});
            }
        },
        Title: {
            create: function(options) {
                return ko.observable(options.data).extend({ required: { message: "Title is required." } });
            }
        },
        Type: {
            create: function(options) {
                return ko.observable(options.data).extend({ required: { message: "Relation is required." } });
            }
        },
        IsPrimaryContact: {
            create: function(options) {
                return ko.observable(options.data);
            }
        }
    }, self);
}


function ContactAddress(contactAddress) {
    var self = this, data;

    data =$.extend({AddressLine1:"",AddressLine2:"",PostCode:"",City:""},contactAddress);
    
    ko.mapping.fromJS(data, {
        AddressRequired: {
            create: function(options) {
                return ko.observable(options.data);
            }
        },
        Id: {
            create: function(options) {
              return ko.observable(options.data);
            }
        },
        AddressLine1: {
            create: function(options) {
               return ko.observable(options.data).extend({
                    required: {
                        message: "Address Line1 required.",
                        onlyIf: function() {
                            return self.AddressRequired();
                        }
                    }
                });
            }
        },
        AddressLine2: {
            create: function(options) {
              return ko.observable(options.data);
            }
        },
        PostCode: {
            create: function(options) {
             return ko.observable(options.data).extend({
                    required: {
                        message: "Post Code required.",
                        onlyIf: function() {
                            return self.AddressRequired();
                        }
                    }
                });
            }
        },
        City: {
            create: function(options) {
                return ko.observable(options.data);
            }
        }
    }, self);
}

function ContactPhone(contactPhone) {
    var self = this, data;

    data =$.extend({HomeNumber:"",MobileNumber:"",WorkNumber:"",Email:""},contactPhone);
    
    ko.mapping.fromJS(data, {
       HomeNumber: {
            create: function(options) {
              return ko.observable(options.data).extend({ number: true });
            }
        },
       WorkNumber: {
            create: function(options) {
              return ko.observable(options.data).extend({ number: true });
            }
        },
       MobileNumber: {
            create: function(options) {
                return ko.observable(options.data).extend({ required: { message: "Mobile Number is required." } }).extend({ mobilephoneUK: true });
            }
        },
       Email: {
            create: function(options) {
             return ko.observable(options.data);
            }
        }

    }, self);    
}

function SessionSubject(sessionSubject) {
    var self = this;

    ko.mapping.fromJS(sessionSubject, {
           SessionId:{
           create :function (options) {
               return ko.observable(options.data);
           }},
           SubjectId:{
           create :function (options) {
               return ko.observable(options.data).extend({ required: {message:"Subject must be selected for checked session"}});
           }}
    },self);
 
}

function SubjectViewModel(subject) {
    var self = this;

    self.Name = subject.Name;
    self.SubjectId = subject.SubjectId;
    self.Level = subject.Level;
    self.IsSelected =ko.observable(subject.IsSelected);
    
    
    //ko.mapping.fromJS(subject,{}, self);
}

function SessionViewModel(session) {
    var self = this;

    
    self.IsSelected = ko.observable(session.IsSelected);
    self.SessionDetails = session.SessionDetails;
    self.SessionId = session.SessionId;
    self.Location = session.Location;
    self.Day = session.Day;
    self.Subjects = ko.observableArray([]);
    
     
     var subjectList = ko.utils.arrayMap(session.Subjects, function(s) {
                    return new SubjectViewModel(s);                    
                });
     self.Subjects.push.apply(self.Subjects,subjectList);
    
    self.IsSelected.subscribe(function(status) {
        if(status == false) {
             ko.utils.arrayForEach(self.Subjects(), function(s) {
                    s.IsSelected(false);
                }); 
        }
    });

    self.SessionSubjectsCheckedValues = function(subjectId) {
                return ko.computed({
                    read: function() {
                        var result =  ko.utils.arrayFilter(self.Subjects(), function(s) {
                            return s.SubjectId == subjectId && s.IsSelected();
                        }).length > 0;
                        return result?subjectId.toString():-1;
                    },
                    write: function(checked) {
                        ko.utils.arrayForEach(self.Subjects(), function(s) {
                            if(s.SubjectId == subjectId) {
                                s.IsSelected(true);
                                return;
                            }
                            s.IsSelected(false);
                        });
                    },
                    owner: self
                });
            };
}

function EducationInstitute(educationInstitute) {
    
    var self = this, data;

    data = $.extend({Name:"",Type:"",Teacher:"",StudentNo:"",From:"",To:"" ,Address:{AddressRequired:false},Qualifications:[]}, educationInstitute);

    ko.mapping.fromJS(data, {
       Name: {
           create: function(options) {
              return ko.observable(options.data).extend({ required:{message:"Institute Name Required"} });
            }
       },
       Type: {
           create: function(options) {
              return ko.observable(options.data).extend({ required:{message:"Institute Type Required"} });
            }
        },
       Teacher: {
           create: function(options) {
              return ko.observable(options.data);
            }    
       },
       StudentNo: {
           create: function(options) {
              return ko.observable(options.data);
            }    
       },
       From: {
           create: function(options) {
              return ko.observable(options.data);
            }    
       },
       To: {
           create: function(options) {
              return ko.observable(options.data);
            }    
       },
       Address: {
           create: function(options) {
              return new ContactAddress(options.data);
            }    
       },
       Qualification: {
            create:function(options) {
                return new Qualification(options.data);
            }
        }
    }, self);

     self.removeQualification = function(qualification) {
               self.Qualifications.remove(qualification);
            };
}

function Qualification(qualification) {
    var self = this,data;
    
    data = $.extend({Subject:"",Result:"",Year:"" }, qualification);
    ko.mapping.fromJS(data, {
        Subject: {
            create:function(options) {
                return ko.observable(options.data);
            }
        },
        Result: {
            create:function(options) {
                return ko.observable(options.data);
            }
        },
        Year: {
            create:function(options) {
                return ko.observable(options.data).extend({ number: true }).extend({ required: { message: "A valid year is required" } });
            }
        }
    },self);
}

function ReturnNullIfUndefined(param) {
    return param == undefined ? null : param;
}

function studentImageDto(studentId, studentImage, studentImageType, errorMessage) {
        var self = this;
        
        self.studentId = studentId;
        self.image = studentImage;
        // self.imageParts = expenseImage.split(";base64,");
        self.image = studentImage;
        self.imageType = studentImageType;
        self.errorMessage = errorMessage;
        
        self.toJson = function () {
            var json = ko.toJSON(self);
            return json;
        };
    }

function StudentViewModel(student, allSessions) {


    var self = this, data;

    student.ImageFile = null;
    student.ImageObjectURL = null;
    student.allSessions = allSessions;

    data = $.extend({
        ConfirmationViewModel: {}
    }, student);

    // initial call to mapping to create the object properties            
    ko.mapping.fromJS(data, {
        MaxYear: {
            create: function (options) {
                return options.data;
            }
        },
        MinYear: {
            create: function (options) {
                return options.data;
            }
        },
        SessionAndSubjects: {
            create: function (options) {
                return new SessionViewModel(options.data);
            }
        },
        EducationInstitutes: {
            create: function (options) {
                return new EducationInstitute(options.data);
            }
        },
        Contacts: {
            create: function (options) {
                return new ContactPerson(options.data);
            }
        },
        FirstName: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "FirstName is required." } });
            }
        },
        LastName: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "LastName is required." } });
            }
        },
        Nationality: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "Nationality is required." } });
            }
        },
        Gender: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "Gender is required." } });
            }
        },
        DateOfBirth: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "DOB is required." } }).extend({ formattedDate: true });
            }
        },
        Curriculum: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "A Curriculum must be selected" } });
            }
        },
        DefaultPaymentPlan: {
            create: function (options) {
                return ko.observable(options.data).extend({ required: { message: "A Payment Plan must be selected" } });
            }
        },
        DiscountAmount: {
            create: function (options) {
                return ko.observable(options.data).extend({ number: true }).extend({ required: { message: "A Discount amount is required" } });;
            }
        },
        AddressVerified: {
            create: function (options) {
                return ko.observable(options.data).extend({ checked: { message: "Address must be verified" } });
            }
        },
        SuffersIllness: {
            create: function (options) {
                return ko.observable(options.data);
            }
        },
        IllnessDetails: {
            create: function (options) {
                return ko.observable(options.data).extend({
                    required: {
                        message: "Illness details is required.",
                        onlyIf: function () {
                            return self.SuffersIllness();
                        }
                    }
                });
            }
        },
        Enabled: {
            create: function (options) {
                return ko.observable(options.data);
            }
        },
        ImageType: {
            create: function (options) {
                return ko.observable(options.data);
            }
        },
        Image: {
            create: function (options) {
                return ko.observable(options.data);
            }
        },
        ImageFile: {
            create: function (options) {
                return ko.observable();
            }
        },
        ImagePath: {
            create: function (options) {
                return ko.observable();
            }
        },
        ImageObjectURL: {
            create: function (options) {
                return ko.observable();
            }
        },
        ConfirmationViewModel: {
            create: function (options) {
                return new ConfirmationViewModel(options.data);
            }
        }
    }, self);

    //check if contact list is empty if so add dummy contact so view is visible 
    if (self.Contacts == undefined || self.Contacts().length < 1) {
        self.Contacts.push(new ContactPerson({ IsPrimaryContact: true }));
    }

    //            if(self.EducationInstitutes == undefined || self.EducationInstitutes().length < 1) {
    //                self.EducationInstitutes.push(new EducationInstitute());
    //            }

    self.ImageSrc = ko.computed(function () {
        return self.ImageType() + "," + self.Image();
    });

    self.addEducationInstitute = function () {
        self.EducationInstitutes.push(new EducationInstitute());
    };

    self.removeEducationInstitute = function (item) {
        self.EducationInstitutes.remove(item);
    };

    self.removeContact = function (item) {
        self.Contacts.remove(item);
    };

    self.addContact = function () {
        self.Contacts.push(new ContactPerson({ IsPrimaryContact: false }));
        //self.errors.showAllMessages(); //had to do this so inputs highlight once added
    };
    self.addQualification = function (educationInstitute) {
        educationInstitute.Qualifications.push(new Qualification("", "", "", educationInstitute));
        //had to do this so inputs highlight once added
    };

    self.paymentPlans = ko.computed(function () {
        var curriculum = self.Curriculum();
        if (curriculum == null)
            return [];
        return ko.utils.arrayFilter(self.allPaymentPlans, function (pp) {
            return pp.Curriculum == parseInt(curriculum);
        });
    }, self);

    self.Sessions = ko.computed(function () {
        var curriculum = self.Curriculum();
        var paymentPlan = self.DefaultPaymentPlan();
        if (curriculum == null || paymentPlan == null)
            return [];
        return ko.utils.arrayFilter(self.SessionAndSubjects(), function (ss) {
            var s = ko.utils.arrayFilter(ss.Subjects(), function (s) {
                return s.Level == curriculum;
            });
            return s.length > 0; //var t = j;
        });
    });
    self.SubjectsByCurriculumAndSession = function (session) {
        return ko.computed(function () {
            var curriculum = self.Curriculum();
            var paymentPlan = self.DefaultPaymentPlan();
            if (curriculum == null || paymentPlan == null)
                return [];

            return ko.utils.arrayFilter(session.Subjects(), function (s) {
                return s.Level
                    == curriculum;
            });
        }, self);
    };


    //self.SessionCheckedValues = ko.observableArray([]);
    self.SessionShouldBeDisabled = function (sessionId) {
        var paymentPlan = ko.utils.arrayFilter(self.paymentPlans(), function (pp) {
            return pp.PaymentPlanId == self.DefaultPaymentPlan();
        })[0];
        return false; //should be diabled if maximum number of check boxes checked or self item is not in the checked list
        //            return self.SessionSubjectsCheckedValues.length == paymentPlan.TotalSessions ||
        //                  (!self.SessionCheckedValues(sessionId));
    };


    self.Cancel = function () {
        location.href = self.SearchStudentsUrl();
    };


    self.PostForm = function () {
        var model = ko.mapping.toJS(self, { ignore: ["ImageSrc", "relationTypes", "curriculumTypes", "allPaymentPlans", "allSessions", "Sessions", "instituteTypes", "ImageFile", "ImageObjectURL", "ImagePath"] });
        var t = ko.utils.stringifyJson(ko.utils.unwrapObservable(model));

        var form = $("#___mainForm");
        var input = $("#model", form);
        input.val(t);
        form.submit();
       //alert("Posting the form");
    }
    self.Save = function () {
        if (self.errors().length < 1) {

            //check if student details firstname/lastname/dob already exists for another student and provide a warning

            jQuery.ajax({
                url: '/Student/SearchStudent',
                type: "POST",
                data: {
                    FirstName: self.FirstName(),
                    LastName: self.LastName(),
                    DateOfBirth: self.DateOfBirth(),
                    ActiveOnly: false,
                    PageSize: 10
                },
                success: function (returnedData) {
                    if (returnedData.Students != null && returnedData.Students.length > 0 && ko.utils.arrayFilter(returnedData.Students, function (s) {

                        return s.StudentId != self.Id();
                    }).length > 0) {
                        //show confirmation  dialog
                        self.ConfirmationViewModel.Show("A student with these details already exists would you like to continue creating/updating this student?", self.PostForm)
                    } else {
                        self.PostForm();
                    }
                }
            });



        } else {
            //ko.validation.group(self, { deep: true, observable: true }).showAllMessages();
            self.errors.showAllMessages();
        }
    };
    self.errors = ko.validation.group(self, { deep: true, observable: true, live: true });
}