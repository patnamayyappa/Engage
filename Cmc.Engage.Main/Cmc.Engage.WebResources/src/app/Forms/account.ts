/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CMC.Engage.account {

    var _lockedControls = [];
  export enum mshied_accounttype {
    Campus = 494280000
    }

    function userIsAdmin() {
            var fetchXml = [
                '<fetch>',
                '  <entity name="systemuser">',
                '    <filter type="and">',
                '      <condition attribute="systemuserid" operator="eq-userid" />',
                '    </filter>',
                '    <link-entity name="systemuserroles" from="systemuserid" to="systemuserid">',
                '      <link-entity name="role" from="roleid" to="roleid" alias="role" >',
                '        <attribute name="roleid"/>',
                '        <attribute name="name"/>',
                '        <filter type="and">',
                '            <condition attribute="name" operator="eq" value="System Administrator" />',
                '        </filter>',
                '      </link-entity>',
                '    </link-entity>',
                '  </entity>',
                '</fetch>'
            ].join('');

            return Xrm.WebApi.retrieveMultipleRecords("systemuser", `?fetchXml=${fetchXml}`)
                .then(
                function success(result) {
                    return result.entities.length > 0;
                },
                function (error) {
                    console.log(error);
                    return false;
                });
        }

    function lockForm(formContext) {
        var accountControls = formContext.ui.controls.getAll();

        accountControls.forEach(function (control) {
            if (!control) {
                return;
            }
            if (control.getDisabled && control.getDisabled()) {
                _lockedControls.push(control);
            }
            if (control.setDisabled) {
                control.setDisabled(true)
            }
        });
    }

    function unlockForm(formContext) {
        var accountControls = formContext.ui.controls.getAll();

        accountControls.forEach(function (control) {
            if (control && control.setDisabled && _lockedControls.indexOf(control) === -1) {
                control.setDisabled(false)
            }
        });

        _lockedControls = [];
    }

    function handleFormLockState(executionContext) {
        var formContext = executionContext.getFormContext();
      var accountType = formContext.getAttribute("mshied_accounttype").getValue();

        if (!accountType) {
            return;
        }

        if (accountType === mshied_accounttype.Campus) {
            lockForm(formContext);
        } else {
            unlockForm(formContext);
        }
    }

    export function onLoad(executionContext) {
        var formContext = executionContext.getFormContext();
      var accountType = formContext.getAttribute('mshied_accounttype');
        userIsAdmin().then(function (systemAdmin) {
            if (!systemAdmin) {
                if (accountType) {
                    accountType.addOnChange(handleFormLockState);
                    handleFormLockState(executionContext);
                }
            }
        })
    }
}
