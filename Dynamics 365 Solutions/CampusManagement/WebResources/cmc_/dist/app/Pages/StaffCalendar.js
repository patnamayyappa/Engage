/// <reference path="../../../node_modules/@types/jquery/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var StaffCalendar;
    (function (StaffCalendar) {
        var _scheduler;
        var _availabilities = [];
        var _editEvent, localizedStrings;
        function executeOnLoad() {
            initializeDisplaySettings();
            $("#locationSelect").change(locationOnChange);
            $("#departmentSelect").change(departmentOnChange);
            $("#userIdSelect").change(userOnChange);
            loadKendoLocalizationFiles();
            initLocationSelect();
            initScheduler();
            $(window).resize(function () {
                var $focusedElement, $scheduler;
                if (_editEvent) {
                    if (document.activeElement) {
                        $focusedElement = $(document.activeElement);
                        $focusedElement.blur();
                    }
                    $scheduler = $("#scheduler").data("kendoScheduler");
                    $scheduler.cancelEvent();
                    if (_editEvent.id) {
                        $scheduler.editEvent(_editEvent);
                    }
                    else {
                        $scheduler.addEvent(_editEvent);
                    }
                    if ($focusedElement) {
                        $($focusedElement[0].tagName + "[name=" + $focusedElement.attr("name") + "]").focus();
                    }
                }
            });
            $("div#scheduler.k-widget.k-scheduler.k-floatwrap").on("touchend", ".k-scheduler-content td:not(.unavailable)", function (e) {
                var $target = $(e.target), now = new Date().getTime(), lastTouchDate = $target.data("last-touch")
                    ? $target.data("last-touch")
                    : null, timeSinceLastTouch = lastTouchDate
                    ? now - lastTouchDate
                    : 0, scheduler, slot;
                if (timeSinceLastTouch < 300 && timeSinceLastTouch > 30) {
                    $target.removeData("last-touch");
                    scheduler = $("#scheduler").data("kendoScheduler");
                    slot = scheduler.slotByElement($target[0]);
                    scheduler.addEvent({ start: slot.startDate, end: slot.endDate });
                }
                else {
                    $target.data("last-touch", now.toString());
                }
            });
        }
        StaffCalendar.executeOnLoad = executeOnLoad;
        function loadKendoLocalizationFiles() {
            try {
                var userLcid = Xrm.Page.context.getUserLcid();
                var $head = $('head'), $script = $('<script>'), $script1 = $('<script>');
                $head.append($script1.attr('type', 'text/javascript').attr('src', "../Scripts/Libraries/kendo.messages." + CampusManagement.LanguageMappings.getCultureName(userLcid) + ".min.js"));
                $head.append($script.attr('type', 'text/javascript').attr('src', "../Scripts/Libraries/kendo.culture." + CampusManagement.LanguageMappings.getCultureName(userLcid) + ".min.js"));
                kendo.culture(CampusManagement.LanguageMappings.getCultureName(userLcid));
            }
            catch (e) {
                console.log("Failed to load LoadKendoCultureFiles with exception : ex -" + e);
            }
        }
        function initializeDisplaySettings() {
            $("title").text(ResourseStrings.StaffCalendar.StaffCalendarTitle);
            $("label[for='location']").text(ResourseStrings.StaffCalendar.Location);
            $("label[for='title']").text(ResourseStrings.StaffCalendar.Title);
            $("label[for='start']").text(ResourseStrings.StaffCalendar.Start);
            $("label[for='end']").text(ResourseStrings.StaffCalendar.End);
            $("label[for='advisor']").text(ResourseStrings.StaffCalendar.Advisor);
            $("label[for='description']").text(ResourseStrings.StaffCalendar.Description);
            $('.text-left').text(ResourseStrings.StaffCalendar.ScheduleAppointment);
            $('#locationSelect option:first').text(ResourseStrings.StaffCalendar.Select);
            $("label[for='department']").text(ResourseStrings.StaffCalendar.Department);
            $('#departmentSelect option:first').text(ResourseStrings.StaffCalendar.Select);
            $('#userIdSelect option:first').text(ResourseStrings.StaffCalendar.Select);
            $('#unavailable').text(ResourseStrings.StaffCalendar.Unavailable);
            $('#available').text(ResourseStrings.StaffCalendar.Available);
            localizedStrings = {
                okButton: ResourseStrings.StaffCalendar.OkButton,
                ErrorPrefix: ResourseStrings.StaffCalendar.ErrorPrefix,
                bookAppointment: ResourseStrings.StaffCalendar.BookAppointment,
            };
        }
        function initScheduler() {
            $("#scheduler").kendoScheduler({
                date: new Date(),
                showWorkHours: false,
                allDaySlot: false,
                footer: false,
                height: 600,
                views: [
                    { type: "day" },
                    { type: "week", selected: true },
                    { type: "month" }
                ],
                editable: {
                    template: $("#customEditorTemplate").html(),
                },
                resize: preventAction,
                resizeEnd: preventAction,
                move: preventAction,
                moveEnd: preventAction,
                edit: editAppointment,
                cancel: clearEditEvent,
                save: createAppointment,
                remove: deleteAppointment,
                navigate: changeView,
                dataBound: function (e) {
                    if (!_scheduler) {
                        return;
                    }
                    if ((e.sender.viewName() === "week" || e.sender.viewName() === "day") && localStorage.getItem("changedView") === "true") {
                        getFacultyMemberAvailability();
                        localStorage.removeItem("changedView");
                    }
                },
                dataSource: {
                    data: new kendo.data.ObservableArray([]),
                    schema: {
                        model: {
                            id: 'id'
                        }
                    }
                },
                resources: [
                    {
                        field: "status",
                        title: "Status",
                        dataSource: [
                            { text: "Pending", value: 5, color: "#51a0ed" },
                            { text: "Complete", value: 3, color: "#ff0000" }
                        ]
                    }
                ]
            });
            _scheduler = $("#scheduler").data("kendoScheduler");
        }
        function locationOnChange(e) {
            var selectedLocation = this.value, selectedDepartment = $("#departmentSelect option:selected").val();
            toggleUserSelect(selectedLocation, selectedDepartment);
        }
        StaffCalendar.locationOnChange = locationOnChange;
        function departmentOnChange(e) {
            var selectedDepartment = this.value, selectedLocation = $("#locationSelect option:selected").val();
            toggleUserSelect(selectedLocation, selectedDepartment);
        }
        StaffCalendar.departmentOnChange = departmentOnChange;
        function toggleUserSelect(selectedLocation, selectedDepartment) {
            var $userSelect = $("#userIdSelect");
            clearSelect($userSelect);
            clearDatasource();
            if (selectedDepartment && selectedLocation) {
                $userSelect.prop("disabled", false);
                initUserSelect();
            }
            else {
                $userSelect.prop("disabled", true);
            }
        }
        function userOnChange(e) {
            kendo.ui.progress($('#scheduler'), true);
            var userId = this.value;
            clearDatasource();
            if (!userId) {
                _availabilities = [];
                grayOutUnavailableSlots();
                return;
            }
            retrieveUserAppointments()
                .then(function (result) {
                filterSchedulerDatasourceByUser(userId, null);
                getFacultyMemberAvailability();
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error);
            });
        }
        StaffCalendar.userOnChange = userOnChange;
        function initLocationSelect() {
            return retrieveLocations()
                .then(function (accounts) {
                var locationSelect = $("#locationSelect"), options = [];
                accounts.entities.forEach(function (account) {
                    options.push($('<option value="' + account.accountid + '">' + account.name + '</option>'));
                });
                locationSelect.append(options);
                initDepartmentSelect();
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error.message);
            });
        }
        function initDepartmentSelect() {
            var $departmentSelect = $("#departmentSelect"), options = [];
            return retrieveDepartments()
                .then(function (departments) {
                departments.entities.forEach(function (department) {
                    options.push($('<option value="' + department.cmc_departmentid + '">' + department.cmc_departmentname + '</option>'));
                });
                $departmentSelect.append(options);
                $departmentSelect.trigger('change');
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error.message);
            });
        }
        function initUserSelect() {
            var $departmentSelect = $('#departmentSelect'), departmentId = $departmentSelect.val(), $locationSelect = $('#locationSelect'), accountId = $locationSelect.val(), // used Jquery val event
            userIdSelect = $("#userIdSelect"), options = [];
            retrieveUsers(departmentId, accountId)
                .then(function (users) {
                var userIds = {};
                users.entities.forEach(function (user) {
                    if (userIds[user.systemuserid]) {
                        return;
                    }
                    userIds[user.systemuserid] = true;
                    options.push($('<option value="' + user.systemuserid + '">' + user.fullname + '</option>'));
                });
                userIdSelect.append(options);
                userIdSelect.trigger('change');
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error.message);
            });
        }
        function retrieveLocations() {
            var fetchXml = [
                '<fetch>',
                '<entity name="account">',
                '<attribute name="name"/>',
                '<filter type="and">',
                '<condition attribute="statecode" operator="eq" value="0" />',
                '<condition attribute="mshied_accounttype" operator="eq" value="494280000" />',
                '</filter>',
                '<order attribute="name" descending="false" />',
                '</entity>',
                '</fetch>'
            ].join('');
            return Xrm.WebApi.retrieveMultipleRecords('account', '?fetchXml=' + fetchXml);
        }
        function retrieveDepartments() {
            var fetchXml = [
                '<fetch>',
                '<entity name="cmc_department">',
                '<attribute name="cmc_departmentname"/>',
                '<filter type="and">',
                '<condition attribute="statecode" operator="eq" value="0" />',
                '</filter>',
                '<order attribute="cmc_departmentname" descending="false" />',
                '</entity>',
                '</fetch>'
            ].join('');
            return Xrm.WebApi.retrieveMultipleRecords('cmc_department', '?fetchXml=' + fetchXml);
        }
        function retrieveUsers(departmentId, accountId) {
            var fetchXml = [
                '<fetch>',
                '<entity name="systemuser">',
                '<attribute name="fullname"/>',
                '<filter type="and">',
                '<condition attribute="cmc_departmentid" operator="eq" value="' + departmentId + '" />',
                '</filter>',
                '<link-entity name="cmc_userlocation" from="cmc_userid" to="systemuserid" alias="userLocation">',
                '<attribute name="cmc_userlocationid"/>',
                '<filter type="and">',
                '<condition attribute="cmc_accountid" operator="eq" value="' + accountId + '" />',
                '</filter>',
                '</link-entity>',
                '<order attribute="fullname" descending="false" />',
                '</entity>',
                '</fetch>'
            ].join('');
            return Xrm.WebApi.retrieveMultipleRecords('systemuser', '?fetchXml=' + fetchXml);
        }
        function getFacultyMemberAvailability() {
            var $selectedUser = $("#userIdSelect option:selected"), userId = $selectedUser.length > 0 ? $selectedUser.val() : null, $selectedAccount = $("#locationSelect option:selected"), accountId = $selectedAccount.length > 0 ? $selectedAccount.val() : null, input = {};
            if (!userId || !accountId || _scheduler.view().name === "month") {
                return;
            }
            input = { "UserId": userId, "AccountId": accountId };
            // The Sonoma library is used for Custom Actions due to issues with Xrm.WebApi.execute
            // in the Unified Interface
            SonomaCmc.WebAPI.post('cmc_RetrieveStaffAvailabilityFromOfficeHours', input)
                .then(function (result) {
                var availabilities = JSON.parse(result.StaffAvailabilityJson);
                _availabilities = availabilities;
                updateTickCounts();
                updateStartAndEndTime();
                _scheduler.view(_scheduler.view().name);
                grayOutUnavailableSlots();
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error);
            });
        }
        function grayOutUnavailableSlots() {
            var allSlots = $(".k-scheduler-table td"), $selectedUser = $("#userIdSelect option:selected"), userId = $selectedUser.length > 0 ? $selectedUser.val() : null, rangesOnDate;
            if (!userId) {
                // No advisor selected, make all slots available
                allSlots.each(function (i, item) {
                    toggleSlot(item, true);
                });
                kendo.ui.progress($('#scheduler'), false);
                return;
            }
            if (_availabilities.length === 0) {
                // No office hours for selected user, make all slots unavailable
                allSlots.each(function (i, item) {
                    toggleSlot(item, false);
                });
                kendo.ui.progress($('#scheduler'), false);
                return;
            }
            // Sort slots by day of month in order to re-use availability ranges for several loop iterations without recalculating
            allSlots.get().sort(compareSlots); //doubt
            allSlots.each(function (i, item) {
                var slot = _scheduler.slotByElement(item), slotDayOfMonth = slot.startDate.getDate();
                if (!rangesOnDate || rangesOnDate.dayOfMonth != slotDayOfMonth) {
                    rangesOnDate = getUnavailableRangesForDateOfMonth(slot.startDate);
                }
                toggleSlot(item, !isUnavailableDuringSlot(slot.startDate, slot.endDate, rangesOnDate), true);
            });
            kendo.ui.progress($('#scheduler'), false);
        }
        function toggleSlot(slot, toggle, showAddIcon) {
            if (showAddIcon === void 0) { showAddIcon = false; }
            if (toggle) {
                $(slot).removeClass("unavailable");
                if (showAddIcon) {
                    if ($(slot).find("i").length <= 0) {
                        var icon = $("<i>").addClass('fa fa-calendar-plus-o').text(" " + localizedStrings.bookAppointment).css({ "color": "green", "font-size": "small" }).click(function (e) {
                            var scheduler = $("#scheduler").data("kendoScheduler");
                            slot = scheduler.slotByElement(e.target.parentElement);
                            scheduler.addEvent({ start: slot.startDate, end: slot.endDate });                      
                        });
                        $(slot).append(icon);
                    }
                }
                else {
                    $(slot).text("");
                }
            }
            else {
                $(slot).addClass("unavailable");
            }
        }
        function isUnavailableDuringSlot(slotStart, slotEnd, rangesOnDate) {
            if (rangesOnDate.allDay) {
                return true;
            }
            return rangesOnDate.unavailableRanges.some(function (range) {
                return doDatesOverlap(range.start, range.end, slotStart, slotEnd);
            });
        }
        function doDatesOverlap(date1Start, date1End, date2Start, date2End) {
            return date1Start < date2End && date1End > date2Start;
        }
        function getUnavailableRangesForDateOfMonth(selectedDate) {
            var rangesOnDate = {
                "dayOfMonth": selectedDate.getDate(),
                "allDay": false,
                "duration": 0,
                "userLocationId": "",
                "unavailableRanges": []
            };
            _availabilities.forEach(function (avail) {
                avail.DateRanges.forEach(function (dateRange) {
                    var start = new Date(Date.parse(dateRange.Start)), end = new Date(Date.parse(dateRange.End));
                    if (!dateRange.IsConflictingAppointmentRange) {
                        start.setTime(start.getTime() + start.getTimezoneOffset() * 60 * 1000);
                        end.setTime(end.getTime() + end.getTimezoneOffset() * 60 * 1000);
                    }
                    if (start.getDate() === selectedDate.getDate() && start.getMonth() === selectedDate.getMonth() && start.getFullYear() === selectedDate.getFullYear()) {
                        rangesOnDate.duration = avail.Duration;
                        rangesOnDate.userLocationId = avail.UserLocationId;
                        rangesOnDate.unavailableRanges.push({ "start": start, "end": end, "duration": avail.Duration, "isConflictingAppointment": dateRange.IsConflictingAppointmentRange });
                    }
                });
            });
            if (rangesOnDate.unavailableRanges.length === 0) {
                rangesOnDate.allDay = true;
            }
            return rangesOnDate;
        }
        function updateTickCounts() {
            var ranges = [], allOfficeHoursDurations = [], distinctOfficeHoursDurations = [], minorTick, needsFifteenMinuteSlots, needsHalfHourSlots, availabilityStartsOrEndsOnQuarterHour = false, availabilityStartsOrEndsOnHalfHour = false;
            if (_scheduler.view().name === "day") {
                ranges.push(getUnavailableRangesForDateOfMonth(_scheduler.view().startDate()));
            }
            else if (_scheduler.view().name === "week") {
                var datesBetweenRange = getDatesBetweenRange(_scheduler.view().startDate(), _scheduler.view().endDate());
                datesBetweenRange.forEach(function (date) {
                    ranges.push(getUnavailableRangesForDateOfMonth(date));
                });
            }
            else {
                return;
            }
            allOfficeHoursDurations = ranges.map(function (range) {
                return range.duration;
            });
            distinctOfficeHoursDurations = allOfficeHoursDurations.filter(function (value, index, self) {
                return self.indexOf(value) === index && value !== 0;
            });
            availabilityStartsOrEndsOnQuarterHour = ranges.some(function (range) {
                var unavailable = range.unavailableRanges;
                return unavailable.some(function (avail) {
                    return !avail.isConflictingAppointment
                        && ((avail.start.getMinutes() % 15 === 0 && avail.start.getMinutes() % 30 !== 0)
                            || (avail.end.getMinutes() % 15 === 0 && avail.end.getMinutes() % 30 !== 0));
                });
            });
            availabilityStartsOrEndsOnHalfHour = ranges.some(function (range) {
                var unavailable = range.unavailableRanges;
                return unavailable.some(function (avail) {
                    return !avail.isConflictingAppointment
                        && ((avail.start.getMinutes() !== 0 && avail.start.getMinutes() % 30 === 0)
                            || (avail.start.getMinutes() !== 0 && avail.end.getMinutes() % 30 === 0));
                });
            });
            needsFifteenMinuteSlots = distinctOfficeHoursDurations.some(function (duration) {
                // returns true if duration is a quarter hour increment AND duration is not a half hour increment, ie .25, .75, or 1.25
                return availabilityStartsOrEndsOnQuarterHour || (duration !== 0 && duration % 0.25 === 0 && duration % 0.5 !== 0);
            });
            needsHalfHourSlots = distinctOfficeHoursDurations.some(function (duration) {
                // returns true if duration is not an hour long AND duration is a half hour increment, ie .5, or 1.5
                return availabilityStartsOrEndsOnHalfHour || (duration !== 0 && duration !== 1 && duration % 0.5 === 0);
            });
            if (needsFifteenMinuteSlots) {
                minorTick = 4;
            }
            else if (needsHalfHourSlots) {
                minorTick = 2;
            }
            else {
                minorTick = 1;
            }
            _scheduler.setOptions({
                minorTickCount: minorTick
            });
        }
        function updateStartAndEndTime() {
            var now = new Date(), earliestAvailableDate = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 6, 0, 0, 0), latestAvailableDate = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 18, 0, 0, 0), earliestAvailableHour = earliestAvailableDate.getHours(), latestAvailableHour = latestAvailableDate.getHours(), originalEarliestHour = earliestAvailableHour, originalLatestHour = latestAvailableHour;
            _availabilities.forEach(function (avail) {
                avail.DateRanges.forEach(function (dateRange) {
                    var start = new Date(Date.parse(dateRange.Start)), end = new Date(Date.parse(dateRange.End));
                    if (!dateRange.IsConflictingAppointmentRange) {
                        start.setTime(start.getTime() + start.getTimezoneOffset() * 60 * 1000);
                        end.setTime(end.getTime() + end.getTimezoneOffset() * 60 * 1000);
                    }
                    if (!dateTimeIsMidnight(end) && end.getHours() < earliestAvailableHour) {
                        earliestAvailableHour = end.getHours();
                    }
                    if (!dateTimeIsMidnight(start) && start.getHours() > latestAvailableHour) {
                        latestAvailableHour = start.getHours();
                    }
                });
            });
            earliestAvailableHour = earliestAvailableHour !== originalEarliestHour ? earliestAvailableHour - 1 : earliestAvailableHour;
            earliestAvailableHour = earliestAvailableHour < 0 ? 0 : earliestAvailableHour;
            latestAvailableHour = latestAvailableHour !== originalLatestHour ? latestAvailableHour + 1 : latestAvailableHour;
            latestAvailableHour = latestAvailableHour > 23 ? 23 : latestAvailableHour;
            _scheduler.setOptions({
                startTime: new Date(earliestAvailableDate.getFullYear(), earliestAvailableDate.getMonth(), earliestAvailableDate.getDate(), earliestAvailableHour, 0, 0),
                endTime: new Date(latestAvailableDate.getFullYear(), latestAvailableDate.getMonth(), latestAvailableDate.getDate(), latestAvailableHour, 0, 0)
            });
        }
        function dateTimeIsMidnight(date) {
            return date.getHours() == 0 && date.getMinutes() == 0;
        }
        function getDatesBetweenRange(startDate, endDate) {
            var dates = [], currentDate = startDate, addDays = function (days) {
                var date = new Date(this.valueOf());
                date.setDate(date.getDate() + days);
                return date;
            };
            while (currentDate <= endDate) {
                dates.push(currentDate);
                currentDate = addDays.call(currentDate, 1);
            }
            return dates;
        }
        function compareSlots(a, b) {
            var slotADate = _scheduler.slotByElement(a).startDate.getDate(), slotBDate = _scheduler.slotByElement(b).startDate.getDate();
            if (slotADate < slotBDate) {
                return -1;
            }
            else if (slotADate > slotBDate) {
                return 1;
            }
            else {
                return 0;
            }
        }
        function createAppointment(e) {
            var selectedUser = $("#userIdSelect option:selected"), userId = selectedUser.length > 0 ? $("#userIdSelect option:selected").val() : null, start = e.event.start, end = e.event.end, title = $('input[name="title"]').val(), description = $('textarea[name="description"]').val(), rangesOfDate = getUnavailableRangesForDateOfMonth(e.event.start), input = {
                "UserId": userId,
                "LocationId": rangesOfDate.userLocationId,
                "StartDate": start.toISOString(),
                "EndDate": end.toISOString(),
                "Title": title,
                "Description": description
            };
            clearEditEvent();
            SonomaCmc.WebAPI.post('cmc_CreateStaffAppointment', input)
                .then(function (result) {
                var appt = JSON.parse(result.StaffAppointmentsJson)[0], data = new kendo.data.SchedulerEvent({
                    id: appt.appointmentId,
                    appointmentId: appt.appointmentId,
                    userId: userId,
                    title: appt.title,
                    start: new Date(Date.parse(appt.start)),
                    end: new Date(Date.parse(appt.end)),
                    status: appt.status,
                });
                removeEmptyIdsFromDataSource();
                _scheduler.dataSource.pushCreate(data);
                getFacultyMemberAvailability();
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error);
            });
        }
        function deleteAppointment(e) {
            var appointmentId = e.event.appointmentId, input = { "AppointmentId": appointmentId };
            return SonomaCmc.WebAPI.post('cmc_DeleteStaffAppointment', input)
                .then(function (result) {
                getFacultyMemberAvailability();
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error);
            });
        }
        function retrieveUserAppointments() {
            var selectedOption = $("#userIdSelect option:selected"), userId = selectedOption.length > 0 ? $("#userIdSelect option:selected").val() : null, input = { "UserId": userId };
            return SonomaCmc.WebAPI.post('cmc_RetrieveStaffAppointments', input)
                .then(function (result) {
                var appointments = JSON.parse(result.StaffAppointmentsJson);
                appointments.forEach(function (appt) {
                    // added code to convertback the ISO to local format.
                    var start = new Date(Date.parse(appt.start)), end = new Date(Date.parse(appt.end));
                    start.setTime(start.getTime() - start.getTimezoneOffset() * 60 * 1000);
                    end.setTime(end.getTime() - end.getTimezoneOffset() * 60 * 1000);
                    var data = new kendo.data.SchedulerEvent({
                        id: appt.appointmentId,
                        appointmentId: appt.appointmentId,
                        userId: userId,
                        title: appt.title ? appt.title : "",
                        start: start,
                        end: end,
                        status: appt.status,
                    });
                    _scheduler.dataSource.pushCreate(data);
                });
            }, function (error) {
                multiLingualAlert(localizedStrings.ErrorPrefix, error);
            });
        }
        function changeView(e) {
            localStorage.setItem("changedView", "true");
        }
        function clearEditEvent() {
            _editEvent = null;
        }
        function editAppointment(e) {
            var rangesOfDate = getUnavailableRangesForDateOfMonth(e.event.start), eventLengthInHours = Math.abs(e.event.end - e.event.start) / 3.6e6, now = new Date(), startIsValidTime, endDate = rangesOfDate.duration !== eventLengthInHours
                ? new Date(e.event.end.setMinutes(e.event.end.getMinutes() + (rangesOfDate.duration - eventLengthInHours) * 60))
                : e.event.end;
            _editEvent = e.event;
            if (_scheduler.view().name === "month") {
                e.preventDefault();
                removeEmptyIdsFromDataSource();
                _scheduler.date(e.event.start);
                _scheduler.view("day");
                return;
            }
            if (!e.event.isNew()) {
                e.preventDefault();
                return;
            }
            if (e.event.start < now) {
                e.preventDefault();
                alert("Cannot schedule an appointment on a past date.");
                return;
            }
            if (isEventOverlappingWithAnyExistingAppointments(e.event.start, endDate, e.event.uid) || isUnavailableDuringSlot(e.event.start, endDate, rangesOfDate)) {
                e.preventDefault();
                alert("Cannot schedule an appointment at this time, the selected staff member is busy.");
                return;
            }
            startIsValidTime = isStartTimeValid(rangesOfDate, e.event.start.getTime());
            if (!startIsValidTime) {
                e.preventDefault();
                alert('Cannot schedule an appointment at this time.  An appointment can only begin every ' + (rangesOfDate.duration) + ' hours from the earliest available time.');
                return;
            }
            if (rangesOfDate.duration !== eventLengthInHours) {
                var start = e.container.find("[name=start][data-role=datetimepicker]"), end = e.container.find("[name=end][data-role=datetimepicker]");
                $(end).data("kendoDateTimePicker").value(endDate);
            }
            // Change event window header from Event to Appointment
            $('.k-window-title').text('Appointment');
            // Default Location/Advisor fields
            $('input[name="location"]').val($("#locationSelect option:selected").text()); // doubt =>$("#locationSelect option:selected")[0].label
            $('input[name="advisor"]').val($("#userIdSelect option:selected").text());
        }
        function isEventOverlappingWithAnyExistingAppointments(start, end, eventUid) {
            var allEvents = _scheduler.dataSource.view();
            return allEvents.some(function (event) {
                return eventUid !== event.uid && doDatesOverlap(start, end, event.start, event.end);
            });
        }
        function isStartTimeValid(rangesOfDate, eventStartTime) {
            var ranges = rangesOfDate.unavailableRanges, earliestAvailable, latestAvailable, validStartTimes = [];
            ranges.forEach(function (range) {
                var start = range.start, end = range.end;
                if (!earliestAvailable || (!dateTimeIsMidnight(end) && end < earliestAvailable)) {
                    earliestAvailable = end;
                }
                if (!latestAvailable || (!dateTimeIsMidnight(start) && start > latestAvailable)) {
                    latestAvailable = start;
                }
            });
            if (!earliestAvailable || !latestAvailable || earliestAvailable > latestAvailable) {
                return [];
            }
            while (earliestAvailable < latestAvailable) {
                if (earliestAvailable.getTime() === eventStartTime) {
                    return true;
                }
                earliestAvailable = new Date(earliestAvailable.setMinutes(earliestAvailable.getMinutes() + (rangesOfDate.duration * 60)));
            }
            return false;
        }
        function filterSchedulerDatasourceByUser(user, filterAll) {
            _scheduler.dataSource.filter({
                operator: function (event) {
                    if (filterAll) {
                        return false;
                    }
                    return event.userId === user;
                }
            });
        }
        // Remove objects from the datasource without an id
        // These objects are added when an event is created, but we are adding our own event manually
        function removeEmptyIdsFromDataSource() {
            var indexes = $.map(_scheduler.dataSource.data(), function (obj, index) {
                if (obj.id == "") {
                    return index;
                }
            });
            while (indexes.length > 0) {
                _scheduler.dataSource.data().splice(indexes.shift(), 1);
            }
        }
        function clearDatasource() {
            var data = _scheduler.dataSource._data;
            for (var i = data.length - 1; i >= 0; i--) {
                var dataItem = data.at(i);
                if (dataItem != null) {
                    data.remove(dataItem);
                }
            }
        }
        function preventAction(e) {
            e.preventDefault();
        }
        function convertDateToLocalIsoString(date) {
            var tzo = -date.getTimezoneOffset(), dif = tzo >= 0 ? '+' : '-', pad = function (num) {
                var norm = Math.abs(Math.floor(num));
                return (norm < 10 ? '0' : '') + norm;
            };
            return date.getFullYear() +
                '-' + pad(date.getMonth() + 1) +
                '-' + pad(date.getDate()) +
                'T' + pad(date.getHours()) +
                ':' + pad(date.getMinutes()) +
                ':' + pad(date.getSeconds()) +
                dif + pad(tzo / 60) +
                ':' + pad(tzo % 60);
        }
        function clearSelect($select) {
            $select.find("option:gt(0)").remove();
            $select.val('');
            $select.trigger('change');
        }
        function multiLingualAlert(defaultMessage, error) {
            Xrm.Navigation.openAlertDialog({
                text: defaultMessage + ': ' + error,
                confirmButtonLabel: localizedStrings.okButton
            }, null);
        }
    })(StaffCalendar = CampusManagement.StaffCalendar || (CampusManagement.StaffCalendar = {}));
})(CampusManagement || (CampusManagement = {}));
