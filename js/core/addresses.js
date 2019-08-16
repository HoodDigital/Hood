"use strict";

if (!$.hood) $.hood = {};
$.hood.Addresses = {
  Init: function Init() {
    $('body').on('click', '.address-set-billing', $.hood.Addresses.SetBilling);
    $('body').on('click', '.address-set-delivery', $.hood.Addresses.SetDelivery);
    $('body').on('click', '.address-delete', $.hood.Addresses.Delete);
  },
  Lists: {
    Address: {
      Loaded: function Loaded(data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#address-list').doesExist()) $.hood.Inline.Reload($('#address-list'), complete);
      }
    }
  },
  Delete: function Delete(e) {
    e.preventDefault();
    var $tag = $(this);

    var deleteAddressCallback = function deleteAddressCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Addresses.Lists.Address.Reload();

          if (data.Success) {
            if ($tag && $tag.data('redirect')) {
              $.hood.Alerts.Success("<strong>Address deleted, redirecting...</strong><br />Just taking you back to the address list.");
              setTimeout(function () {
                window.location = $tag.data('redirect');
              }, 1500);
            }
          }
        });
      }
    };

    $.hood.Alerts.Confirm("The address will be permanently removed.", "Are you sure?", deleteAddressCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
  },
  CreateOrEdit: function CreateOrEdit() {
    $.hood.Google.Addresses.InitAutocomplete();
    $('#address-form').hoodValidator({
      validationRules: {
        Number: {
          required: true
        },
        Address1: {
          required: true
        },
        City: {
          required: true
        },
        County: {
          required: true
        },
        Postcode: {
          required: true
        },
        Country: {
          required: true
        }
      },
      submitButtonTag: $('#address-form-submit'),
      submitUrl: $('#address-form').attr('action'),
      submitFunction: function submitFunction(data) {
        $.hood.Helpers.ProcessResponse(data);
        $.hood.Addresses.Lists.Address.Reload();

        if (data.Success) {
          $.hood.Inline.CloseModal();
        }
      }
    });
  },
  SetBilling: function SetBilling(e) {
    e.preventDefault();
    var $tag = $(this);

    var setBillingAddressCallback = function setBillingAddressCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Addresses.Lists.Address.Reload();
        });
      }
    };

    $.hood.Alerts.Confirm("The current billing address will be overwritten.", "Are you sure?", setBillingAddressCallback, 'error');
  },
  SetDelivery: function SetDelivery(e) {
    e.preventDefault();
    var $tag = $(this);

    var setDeliveryAddressCallback = function setDeliveryAddressCallback(isConfirm) {
      if (isConfirm) {
        $.post($tag.attr('href'), function (data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Addresses.Lists.Address.Reload();
        });
      }
    };

    $.hood.Alerts.Confirm("The current delivery address will be overwritten.", "Are you sure?", setDeliveryAddressCallback, 'error');
  }
};
$(document).ready($.hood.Addresses.Init);