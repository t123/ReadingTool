$.ajaxSetup({ scriptCharset: "utf-8", contentType: "application/x-www-form-urlencoded; charset=UTF-8" });
$.ajaxSettings.traditional = true;
$.ajaxSetup({ type: "post" });

function messageCount(url) {
    $.post(
        url,
        function (data) {
            if(data=='') {
                $('#msgCount').hide();
            } else {
                $('#msgCount').show().html(data);
            }
            
        }
    );
}

//General UI helper for tags and autocomplete, plus some stuff for add/editing of texts
function uiHelper(settings) {
    var self = this;
    self.settings = settings;
    self.settings.autocomplete = settings.autocomplete || [];
    self.settings.tags = settings.tags || [];

    self.split = function (val) { return val.split(/ \s*/); };
    self.extractLast = function (term) { return self.split(term).pop(); };

    $.each(self.settings.autocomplete, function (i, item) {
        $(item.name).autocomplete({
            source: self.settings.urls.ajaxUrl + '/' + item.endpoint,
            minLength: item.length || 3,
            select: function (event, ui) {
                if (item.vm != null) {//TODO UUUGH
                    if (item.type == 'collection') {
                        item.vm.newCollectionName($(this).val(ui.item.value).val());
                    }
                    else if (item.type == 'groupshare') {
                        item.vm.groupName($(this).val(ui.item.value).val());
                    } else if (item.type == 'groupunshare') {
                        item.vm.ungroupName($(this).val(ui.item.value).val());
                    }
                }
            }
        });
    });

    $.each(self.settings.tags, function (i, item) {
        $(item.name)
			.bind("keydown", function (event) {
			    if (event.keyCode === $.ui.keyCode.TAB &&
					$(this).data("autocomplete").menu.active) {
			        event.preventDefault();
			    }
			})
			.autocomplete({
			    source: function (request, response) {
			        $.post(self.settings.urls.ajaxUrl + '/' + item.endpoint, {
			            term: self.extractLast(request.term)
			        }, response);
			    },
			    search: function () {
			        var term = self.extractLast(this.value);
			        if (term.length < (item.length || 2)) {
			            return false;
			        }
			    },
			    focus: function () { return false; },
			    select: function (event, ui) {
			        var terms = self.split(this.value);
			        terms.pop();
			        terms.push(ui.item.value);
			        terms.push("");
			        this.value = terms.join(" ");

			        if (item.vm != null) {//TODO UUUGH 
			            if (item.type == 'tag') {
			                item.vm.tagsToAdd(this.value);
			            }
			            else if (item.type == 'untag') {
			                item.vm.tagsToRemove(this.value);
			            }
			        }
			        return false;
			    }
			});
    });

    $('#ParallelIsRtl').click(function () { $('#ParallelText').attr('dir', $(this).is(":checked") ? 'rtl' : 'ltr'); });

    $("#addPart").click(function () {
        $.ajax({
            url: self.settings.urls.textUrl + '/addtextpart',
            cache: false,
            success: function (html) {
                $("#parts").append(html);
                $('form').removeData('validator');
                $.validator.unobtrusive.parse($('form'));
            }
        });
        return false;
    });

    $(".deletePart").live("click", function () {
        $('form').removeData('validator');
        $(this).parents("div.part:first").remove();
        $.validator.unobtrusive.parse($('form'));
        return false;
    });
}

///MODELS
/////////

function groupItemModel(data) {
    this.id = data.id;
    this.title = data.title;
    this.collectionName = data.collectionName;
    this.collectionNo = data.collectionNo;
    this.hasAudio = data.hasAudio;
    this.isParallelText = data.isParallel;
    this.isText = data.isText;
    this.language = data.language;
}

function itemModel(data) {
    this.id = data.id;
    this.language = data.language;
    this.languageColour = data.languageColour;
    this.title = data.title;
    this.collectionName = ko.observable(data.collectionName);
    this.collectionNo = data.collectionNo;
    this.lastSeen = data.lastSeen;
    this.itemType = data.itemType;
    this.isShared = ko.observable(data.isShared);
    this.sharedGroups = ko.observable(data.sharedGroups);
    this.isParallel = data.isParallel;
    this.hasAudio = data.hasAudio;
}

function groupModel(data) {
    this.id = data.id;
    this.name = data.name;
    this.association = data.association;
    this.canManage = data.canManage;
    this.canView = data.canView;
    this.canEdit = data.canEdit;
    this.canInfo = data.canInfo;
    this.pending = data.pending == "" ? "" : " (" + data.pending + " requests)";
}

function groupMembershipModel(data) {
    this.id = data.id;
    this.type = data.type;
    this.user = new userSimpleModel(data.user);
}

function userSimpleModel(data) {
    if (data == null || data == undefined)
        return;

    this.id = data.id;
    this.username = data.username;
    this.name = data.name;
}

function messageModel(data) {
    this.id = data.id;
    this.from = new userSimpleModel(data.from);
    var self = this;
    this.to = [];

    if (data.to != null && data.to != undefined) {
        data.to.forEach(function (item) {
            self.to.push(new userSimpleModel(item));
        });
    }

    this.isStarred = ko.observable(data.isStarred);
    this.isRead = data.isRead;
    this.subject = data.subject;
    this.date = data.date;
}

function languageModel(data) {
    this.id = data.id;
    this.name = data.name;
    this.colour = data.colour;
    this.systemName = data.systemName;
}

function collectionNameModel(data) {
    this.id = data == '' ? '(no collection name)' : data;
    this.name = data == '' ? '(no collection name)' : data;
}

function tagModel(data) {
    this.name = data;
}

function wordModel(data) {
    this.id = data.id;
    this.languageName = data.languageName;
    this.languageColour = data.languageColour;
    this.word = data.word;
    this.state = ko.observable(data.state);
    this.box = data.box;
    this.sentence = data.sentence;
}

//Specifically for adding texts
function textaddLanguageModel(language) {
    this.id = language.id;
    this.isRtl = language.isRtl;
    this.code = language.code;
    this.name = language.name;
}


////VIEWS
/////////////
//View model for addings texts
function addTextVM(settings) {
    var self = this;
    self.settings = settings;
    self.languages = ko.observableArray([]);
    self.selectedLanguage = ko.observableArray([]);

    self.init = function () {
        $.post(
			self.settings.urls.ajaxUrl + '/languagesfortexts',
			{ __RequestVerificationToken: self.settings.afToken },
			function (data) {
			    data.languages.forEach(function (language) {
			        self.languages.push(new textaddLanguageModel(language));
			    });

			    self.selectedLanguage.push(self.settings.languageId);
			}
		);
    };

    self.canParse = ko.computed(function () {
        if (self.selectedLanguage() == undefined) return false;
        var l = ko.utils.arrayFirst(self.languages(), function (item) {
            return item.id == self.selectedLanguage();
        });

        return l != null && l.code == 'jpn';
    }, self);

    self.isRtl = ko.computed(function () {
        if (self.selectedLanguage() == undefined) return 'ltr';
        var l = ko.utils.arrayFirst(self.languages(), function (item) {
            return item.id == self.selectedLanguage();
        });

        return l != null && l.isRtl ? 'rtl' : 'ltr';
    });

    self.init();
    ko.applyBindings(self);
}

function groupMembershipVM(settings) {
    var self = this;
    self.settings = settings;
    self.items = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.totalItems = ko.observable(0);
    self.totalPages = ko.observable(1);

    self.selectedFolders = ko.observableArray([]);
    self.textFilter = ko.observable('');

    self.init = function () {
        ko.applyBindings(self);
    };

    self.paging = ko.computed(function () {
        if (self.totalPages() > 12) {
            var start = self.currentPage() - 6;
            var end = self.currentPage() + 6;
            var pages = [];

            if (start < 2) {
                start = 1;
            } else {
                pages.push(1);
            }

            var showEnd = true;
            if (end > self.totalPages() - 1) {
                end = self.totalPages();
                showEnd = false;
            }

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }

            if (showEnd) {
                pages.push(self.totalPages());
            }

            return pages;
        }

        return ko.utils.range(1, self.totalPages);
    }, self);

    self.changePage = function (page) {
        self.currentPage(page);
    };

    self.filter = ko.computed(function () {
        $.post(
			self.settings.url + '/searchmembership',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    groupId: self.settings.groupId,
			    folders: self.selectedFolders(),
			    page: self.currentPage()
			},
			function (data) {
			    self.items.removeAll();
			    data.items.forEach(function (item) {
			        self.items.push(new groupMembershipModel(item));
			    });

			    self.totalItems(data.totalItems);
			    self.totalPages(data.totalPages);
			});
    }, self).extend({ throttle: 500 });

    self.init();
}

function groupsVM(settings) {
    self.settings = settings;
    self.items = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.totalItems = ko.observable(0);
    self.totalPages = ko.observable(1);

    self.selectedFolders = ko.observableArray([]);
    self.textFilter = ko.observable('');

    self.init = function () {
        ko.applyBindings(self);
    };

    self.paging = ko.computed(function () {
        if (self.totalPages() > 12) {
            var start = self.currentPage() - 6;
            var end = self.currentPage() + 6;
            var pages = [];

            if (start < 2) {
                start = 1;
            } else {
                pages.push(1);
            }

            var showEnd = true;
            if (end > self.totalPages() - 1) {
                end = self.totalPages();
                showEnd = false;
            }

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }

            if (showEnd) {
                pages.push(self.totalPages());
            }

            return pages;
        }

        return ko.utils.range(1, self.totalPages);
    }, self);

    self.changePage = function (page) {
        self.currentPage(page);
    };

    /*
    self.showModal = function (item) {
    $.post(
    self.settings.url + '/groupinfo',
    {
    __RequestVerificationToken: self.settings.afToken,
    groupId: item.id
    },
    function (data) {
    if (data != null && data.result == 'OK') {
    $("#dialog-info").dialog(
    {
    height: 250,
    width: 400,
    tilte: data.result.name
    }
    );
    } 
    }
    );
    };
    */

    self.filter = ko.computed(function () {
        $.post(
			self.settings.url + '/search',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    filter: self.textFilter(),
			    folders: self.selectedFolders(),
			    page: self.currentPage()
			},
			function (data) {
			    self.items.removeAll();
			    data.items.forEach(function (item) {
			        self.items.push(new groupModel(item));
			    });

			    self.totalItems(data.totalItems);
			    self.totalPages(data.totalPages);
			});
    }, self).extend({ throttle: 500 });

    self.init();
}

function groupViewVM(settings) {
    var self = this;
    self.settings = settings;
    self.items = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.totalItems = ko.observable(0);
    self.totalPages = ko.observable(1);
    self.limits = ko.observableArray([25, 50, 100, 250, 500, 1000]);
    self.limit = ko.observable(25);

    self.selectedFolders = ko.observableArray(['texts']);
    self.textFilter = ko.observable('');

    self.init = function () {
        ko.applyBindings(self);
    };

    self.paging = ko.computed(function () {
        if (self.totalPages() > 12) {
            var start = self.currentPage() - 6;
            var end = self.currentPage() + 6;
            var pages = [];

            if (start < 2) {
                start = 1;
            } else {
                pages.push(1);
            }

            var showEnd = true;
            if (end > self.totalPages() - 1) {
                end = self.totalPages();
                showEnd = false;
            }

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }

            if (showEnd) {
                pages.push(self.totalPages());
            }

            return pages;
        }

        return ko.utils.range(1, self.totalPages);
    }, self);

    self.changePage = function (page) {
        self.currentPage(page);
        self.checkAll(false);
    };

    self.filter = ko.computed(function () {
        $.post(
            self.settings.url + '/groupitems',
            {
                __RequestVerificationToken: self.settings.afToken,
                groupId: self.settings.groupId,
                filter: self.textFilter(),
                folders: self.selectedFolders(),
                limit: self.limit(),
                page: self.currentPage()
            },
            function (data) {
                self.items.removeAll();
                data.items.forEach(function (item) {
                    self.items.push(new groupItemModel(item));
                });

                self.totalItems(data.totalItems);
                self.totalPages(data.totalPages);
            });
    }, self).extend({ throttle: 500 });

    self.init();
}

function languagesVM(settings) {
    var self = this;
    self.settings = settings;
    self.items = ko.observableArray([]);
    self.notSetLanguages = ko.observable(false);

    self.init = function () {
        $.post(
			self.settings.url + '/search',
			{
			    __RequestVerificationToken: self.settings.afToken
			},
			function (data) {
			    self.items.removeAll();
			    data.items.forEach(function (item) {
			        self.items.push(new languageModel(item));
			        if (item.systemName == 'Not Yet Set') self.notSetLanguages(true);
			    });
			});

        ko.applyBindings(self);
    };

    self.deleteLanguage = function (language) {
        if (!confirm('Are you sure you want to delete this. This will also delete all the texts/videos in this language')) return;

        $.post(
			self.settings.url + '/deletelanguage',
			{ id: language.id, __RequestVerificationToken: self.settings.afToken },
			function (data) {
			    if (data != null && data == "OK") {
			        self.items.remove(function (item) { return item.id == language.id; });
			    }
			}
		);
    };

    self.init();
}

function messagesVM(settings) {
    var self = this;
    self.settings = settings;
    self.items = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.totalItems = ko.observable(0);
    self.totalPages = ko.observable(1);

    self.selectedFolders = ko.observable('inbox');
    self.textFilter = ko.observable('');

    self.init = function () {
        ko.applyBindings(self);
    };

    self.paging = ko.computed(function () {
        if (self.totalPages() > 12) {
            var start = self.currentPage() - 6;
            var end = self.currentPage() + 6;
            var pages = [];

            if (start < 2) {
                start = 1;
            } else {
                pages.push(1);
            }

            var showEnd = true;
            if (end > self.totalPages() - 1) {
                end = self.totalPages();
                showEnd = false;
            }

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }

            if (showEnd) {
                pages.push(self.totalPages());
            }

            return pages;
        }

        return ko.utils.range(1, self.totalPages);
    }, self);

    self.changePage = function (page) {
        self.currentPage(page);
    };

    self.filter = ko.computed(function () {
        var direction = 'in';
        if (self.selectedFolders() == 'sent') {
            direction = 'out';
        }

        $.post(
			self.settings.url + '/search' + direction,
			{
			    __RequestVerificationToken: self.settings.afToken,
			    filter: self.textFilter(),
			    folders: self.selectedFolders(),
			    page: self.currentPage()
			},
			function (data) {
			    self.items.removeAll();
			    data.items.forEach(function (item) {
			        self.items.push(new messageModel(item));
			    });

			    self.totalItems(data.totalItems);
			    self.totalPages(data.totalPages);
			});
    }, self).extend({ throttle: 500 });

    self.star = function (item) {
        self.changeStar(item.id, true);
    };

    self.unstar = function (item) {
        self.changeStar(item.id, false);
    };

    self.changeStar = function (id, status) {
        $.post(
			self.settings.url + '/star',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    id: id,
			    status: status
			},
			function (data) {
			    var item = ko.utils.arrayFirst(self.items(), function (iter) {
			        return iter.id == data.id;
			    });

			    if (item != null) {
			        item.isStarred(data.status);
			    }
			});
    };

    self.deleteMessage = function (item) {
        if (!confirm('Are you sure you want to delete this message?')) {
            return;
        }

        $.post(
			self.settings.url + '/delete',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    id: item.id
			},
			function (data) {
			    self.items.remove(function (item) { return item.id == data; });
			});
    };

    self.init();
}

function mergedVM(settings) {
    var self = this;
    self.settings = settings;
    self.items = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.totalItems = ko.observable(0);
    self.totalPages = ko.observable(1);
    self.checkAll = ko.observable(false);
    self.limits = ko.observableArray([25, 50, 100, 250, 500, 1000]);
    self.limit = ko.observable(25);
    self.states = ko.observableArray(['Known', 'Unknown', 'Not Seen', 'Ignored']);
    self.newState = ko.observable('');
    self.orderBy = ko.observable('language');
    self.orderDirection = ko.observable('asc');
    self.collectionNames = ko.observableArray(['(none)']);
    self.itemTypes = ko.observableArray(['Texts', 'Videos']);

    self.selectedItemTypes = ko.observableArray(['Texts', 'Videos']);
    self.selectedItems = ko.observableArray([]);
    self.selectedLanguages = ko.observableArray([]);
    self.selectedCollections = ko.observableArray([]);
    self.textFilter = ko.observable('');
    self.selectedStates = ko.observableArray([]);
    self.selectedBoxes = ko.observableArray([]);

    self.tagsToAdd = ko.observable('');
    self.tagsToRemove = ko.observable('');
    self.newCollectionName = ko.observable('');
    self.groupName = ko.observable('');
    self.ungroupName = ko.observable('');

    self.showAddTagsDD = function () { $('#addTagsDD').toggle(); };
    self.showRemoveTagsDD = function () { $('#removeTagsDD').toggle(); };
    self.showChangeCollectionDD = function () { $('#changeCollectionDD').toggle(); };
    self.showShareDD = function () { $('#shareDD').toggle(); };
    self.showUnshareDD = function () { $('#unshareDD').toggle(); };
    self.showDeleteDD = function () { $('#deleteDD').toggle(); };
    self.showChangeStateDD = function () { $('#changeStateDD').toggle(); };


    self.init = function () {
        ko.applyBindings(self);
    };

    self.changeOrder = function (order, item, event) {
        var element = $(event.currentTarget);
        $('#headerRow th a').removeClass('sprite-downarrow').addClass('sprite-uparrow');

        if (self.orderBy() == order) {
            if (self.orderDirection() == 'asc') {
                self.orderDirection('desc');
                element.removeClass('sprite-uparrow').addClass('sprite-downarrow');
            } else {
                self.orderDirection('asc');
            }
        } else {
            self.orderDirection('asc');
            self.orderBy(order);
        }
        self.changePage(1);
    };

    self.paging = ko.computed(function () {
        if (self.totalPages() > 12) {
            var start = self.currentPage() - 6;
            var end = self.currentPage() + 6;
            var pages = [];

            if (start < 2) {
                start = 1;
            } else {
                pages.push(1);
            }

            var showEnd = true;
            if (end > self.totalPages() - 1) {
                end = self.totalPages();
                showEnd = false;
            }

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }

            if (showEnd) {
                pages.push(self.totalPages());
            }

            return pages;
        }

        return ko.utils.range(1, self.totalPages);
    }, self);

    self.changePage = function (page) {
        self.selectedItems.removeAll();
        self.currentPage(page);
        self.checkAll(false);
    };

    self.selectAll = function () {
        if (self.checkAll()) {
            self.selectedItems(self.items().map(function (item) { return item.id; }));
        } else {
            self.selectedItems.removeAll();
        }

        return true;
    };

    self.shareWithGroup = function () {
        $('#shareGroupStatus').show();
        $.post(
		self.settings.url + '/share',
		{
		    __RequestVerificationToken: self.settings.afToken,
		    items: self.selectedItems(),
		    share: 'share',
		    groupName: self.groupName()
		},
		function (data) {
		    $('#shareGroupStatus').hide();

		    if (data != null && data.result == 'OK') {
		        data.updates.forEach(function (selectedItem) {
		            var found = ko.utils.arrayFirst(self.items(), function (item) {
		                return item.id == selectedItem.Key;
		            });

		            if (found != null) {
		                found.isShared(selectedItem.Value.IsShared);
		                found.sharedGroups(selectedItem.Value.SharedGroups);
		            }
		        });

		        $("#shareDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
		        self.groupName('');
		    } else {
		        $("#shareDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
		    }
		});
    };

    self.unshareFromGroup = function () {
        $('#unshareGroupStatus').show();
        $.post(
		self.settings.url + '/share',
		{
		    __RequestVerificationToken: self.settings.afToken,
		    items: self.selectedItems(),
		    share: 'unshare',
		    groupName: self.ungroupName()
		},
		function (data) {
		    $('#unshareGroupStatus').hide();

		    if (data != null && data.result == 'OK') {
		        data.updates.forEach(function (selectedItem) {
		            var found = ko.utils.arrayFirst(self.items(), function (item) {
		                return item.id == selectedItem.Key;
		            });

		            if (found != null) {
		                found.isShared(selectedItem.Value.IsShared);
		                found.sharedGroups(selectedItem.Value.SharedGroups);
		            }
		        });

		        $("#unshareDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
		        self.ungroupName('');
		    } else {
		        $("#unshareDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
		    }
		});
    };

    self.addTags = function () {
        $('#addTagsStatus').show();
        $.post(
			self.settings.url + '/addtags',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    items: self.selectedItems(),
			    tagsToAdd: self.tagsToAdd()
			},
			function (data) {
			    $('#addTagsStatus').hide();

			    if (data == 'OK') {
			        $("#addTagsDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
			        self.tagsToAdd('');
			    } else {
			        $("#addTagsDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
			    }
			});
    };

    self.removeTags = function () {
        $('#removeTagsStatus').show();
        $.post(
			self.settings.url + '/removetags',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    items: self.selectedItems(),
			    tagsToRemove: self.tagsToRemove()
			},
			function (data) {
			    $('#removeTagsStatus').hide();

			    if (data == 'OK') {
			        $("#removeTagsDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
			        self.tagsToRemove('');
			    } else {
			        $("#removeTagsDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
			    }
			});
    };

    self.changeCollectionName = function () {
        $('#rchangeCollectionStatus').show();
        $.post(
			self.settings.url + '/changecollectionname',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    items: self.selectedItems(),
			    newCollectionName: self.newCollectionName()
			},
			function (data) {
			    $('#changeCollectionStatus').hide();

			    if (data == 'OK') {
			        self.selectedItems().forEach(function (selectedItem) {
			            var found = ko.utils.arrayFirst(self.items(), function (item) {
			                return item.id == selectedItem;
			            });

			            if (found != null) {
			                found.collectionName(self.newCollectionName());
			            }
			        });

			        $("#changeCollectionDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
			        self.newCollectionName('');
			    } else {
			        $("#changeCollectionDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
			    }
			});
    };

    self.deleteItem = function (item) {
        if (!confirm('Are you sure you want to delete this?'))
            return;

        $.post(
                self.settings.url + '/delete',
                {
                    __RequestVerificationToken: self.settings.afToken,
                    items: [item.id]
                },
                function (data) {
                    if (data == 'OK') {
                        self.items.remove(function (x) {
                            return x.id == item.id;
                        });
                    }
                });

    };

    self.deleteItems = function () {
        $('#deleteStatus').show();
        $.post(
			self.settings.url + '/delete',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    items: self.selectedItems()
			},
			function (data) {
			    $('#deleteStatus').hide();

			    if (data == 'OK') {
			        $("#deleteDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
			        self.currentPage(0);
			        self.currentPage(1);
			    } else {
			        $("#deleteDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
			    }
			});
    };

    self.changeStatus = function () {
        $('#changeStateStatus').show();
        $.post(
			self.settings.url + '/changestatus',
			{
			    __RequestVerificationToken: self.settings.afToken,
			    items: self.selectedItems(),
			    newState: self.newState()
			},
			function (data) {
			    $('#changeStateStatus').hide();

			    if (data == 'OK') {
			        self.selectedItems().forEach(function (selectedItem) {
			            var found = ko.utils.arrayFirst(self.items(), function (item) {
			                return item.id == selectedItem;
			            });

			            if (found != null) {
			                found.state(self.newState());
			            }
			        });

			        $("#changeStateDD").stop().css("background-color", "white").animate({ backgroundColor: "#E9FFD6" }, 1500);
			        self.newState('');
			    } else {
			        $("#changeStateDD").stop().css("background-color", "white").animate({ backgroundColor: "#FFF4F4" }, 1500);
			    }
			});
    };

    self.filter = ko.computed(function () {
        self.selectedItems([]);

        if (settings.index == 'words') {
            $.post(
					self.settings.url + '/search',
					{
					    __RequestVerificationToken: self.settings.afToken,
					    filter: self.textFilter(),
					    languages: self.selectedLanguages(),
					    states: self.selectedStates(),
					    boxes: self.selectedBoxes(),
					    orderBy: self.orderBy(),
					    orderDirection: self.orderDirection(),
					    limit: self.limit(),
					    page: self.currentPage()
					},
					function (data) {
					    self.items.removeAll();
					    data.items.forEach(function (item) {
					        self.items.push(new wordModel(item));
					    });

					    self.totalItems(data.totalItems);
					    self.totalPages(data.totalPages);
					});
        } else if (settings.index == 'items') {
            self.selectedItems([]);
            $.post(
                self.settings.url + '/search',
                {
                    __RequestVerificationToken: self.settings.afToken,
                    types: self.selectedItemTypes(),
                    filter: self.textFilter(),
                    languages: self.selectedLanguages(),
                    collectionNames: self.selectedCollections(),
                    orderBy: self.orderBy(),
                    orderDirection: self.orderDirection(),
                    limit: self.limit(),
                    page: self.currentPage()
                },
                function (data) {
                    self.items.removeAll();
                    data.items.forEach(function (item) {
                        self.items.push(new itemModel(item));
                    });

                    if (data.collectionNames != null) {
                        self.collectionNames.removeAll();
                        self.collectionNames.push('(none)');
                        data.collectionNames.forEach(function (cn) {
                            self.collectionNames.push(cn);
                        });
                    }

                    self.totalItems(data.totalItems);
                    self.totalPages(data.totalPages);
                });
        }
    }, self).extend({ throttle: 500 });

    self.init();
}