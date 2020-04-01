"use strict";

if (!$.hood) $.hood = {};
$.hood.Subscriptions = {
  Init: function Init() {
    $('body').on('click', '.subscriptions-subs-delete', $.hood.Subscriptions.Subscriptions.Delete);
    $('body').on('click', '.subscriptions-plans-delete', $.hood.Subscriptions.Plans.Delete);
    if ($('#subscriptions-plans-edit-form').doesExist()) $.hood.Subscriptions.Plans.Edit();
    $('body').on('click', '.subscriptions-products-delete', $.hood.Subscriptions.Products.Delete);
    if ($('#subscriptions-products-edit-form').doesExist()) $.hood.Subscriptions.Products.Edit();
    if ($('#subscriptions-stripe-edit-form').doesExist()) $.hood.Subscriptions.Stripe.Edit();
  },
  Lists: {
    Plans: {
      Loaded: function Loaded(sender, data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#subscriptions-plans-list').doesExist()) $.hood.Inline.Reload($('#subscriptions-plans-list'), complete);
      }
    },
    Stripe: {
      Loaded: function Loaded(sender, data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#subscriptions-stripe-list').doesExist()) $.hood.Inline.Reload($('#subscriptions-stripe-list'), complete);
      }
    },
    StripeProducts: {
      Loaded: function Loaded(sender, data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#subscriptions-stripe-products-list').doesExist()) $.hood.Inline.Reload($('#subscriptions-stripe-products-list'), complete);
      }
    },
    Products: {
      Loaded: function Loaded(sender, data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#subscriptions-products-list').doesExist()) $.hood.Inline.Reload($('#subscriptions-products-list'), complete);
      }
    },
    Subscribers: {
      Loaded: function Loaded(sender, data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#subscriptions-subscribers-list').doesExist()) $.hood.Inline.Reload($('#subscriptions-subscribers-list'), complete);
      }
    }
  },
  Subscriptions: {
    Delete: function Delete(e) {
      e.preventDefault();
      var $tag = $(this);

      var deleteSubscriptionCallback = function deleteSubscriptionCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Subscriptions.Lists.Subscribers.Reload();

            if (data.Success) {
              if ($tag && $tag.data('redirect')) {
                $.hood.Alerts.Success("<strong>Subscription deleted, redirecting...</strong><br />Just taking you back to the subscription list.");
                setTimeout(function () {
                  window.location = $tag.data('redirect');
                }, 1500);
              }
            }
          });
        }
      };

      $.hood.Alerts.Confirm("The subscription will be removed from the site, however record of the subscription will remain in your Stripe account.", "Are you sure?", deleteSubscriptionCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
    }
  },
  Plans: {
    Delete: function Delete(e) {
      e.preventDefault();
      var $tag = $(this);

      var deletePlanCallback = function deletePlanCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Subscriptions.Lists.Plans.Reload();

            if (data.Success) {
              if ($tag && $tag.data('redirect')) {
                $.hood.Alerts.Success("<strong>Plan deleted, redirecting...</strong><br />Just taking you back to the subscription plan list.");
                setTimeout(function () {
                  window.location = $tag.data('redirect');
                }, 1500);
              }
            }
          });
        }
      };

      $.hood.Alerts.Confirm("The plan will be permanently removed.", "Are you sure?", deletePlanCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
    },
    Create: function Create() {
      $('#subscriptions-plans-create-form').hoodValidator({
        validationRules: {
          Name: {
            required: true
          },
          Description: {
            required: true
          },
          CreatePrice: {
            required: true
          },
          Currency: {
            required: true
          },
          Interval: {
            required: true
          },
          IntervalCount: {
            required: true
          }
        },
        submitButtonTag: $('#subscriptions-plans-create-submit'),
        submitUrl: $('#subscriptions-plans-create-form').attr('action'),
        submitFunction: function submitFunction(data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Subscriptions.Lists.Plans.Reload();
        }
      });
    }
  },
  Products: {
    Delete: function Delete(e) {
      e.preventDefault();
      var $tag = $(this);

      var deleteProductCallback = function deleteProductCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Subscriptions.Lists.Products.Reload();

            if (data.Success) {
              if ($tag && $tag.data('redirect')) {
                $.hood.Alerts.Success("<strong>Product deleted, redirecting...</strong><br />Just taking you back to the subscription product list.");
                setTimeout(function () {
                  window.location = $tag.data('redirect');
                }, 1500);
              }
            }
          });
        }
      };

      $.hood.Alerts.Confirm("The product will be permanently removed.", "Are you sure?", deleteProductCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
    },
    Create: function Create() {
      $('#susbcriptions-products-create-form').find('.datepicker').datetimepicker({
        locale: 'en-gb',
        format: 'L'
      });
      $('#susbcriptions-products-create-form').hoodValidator({
        validationRules: {
          DisplayName: {
            required: true
          }
        },
        submitButtonTag: $('#susbcriptions-products-create-submit'),
        submitUrl: $('#susbcriptions-products-create-form').attr('action'),
        submitFunction: function submitFunction(data) {
          $.hood.Helpers.ProcessResponse(data);
          $.hood.Subscriptions.Lists.Products.Reload();
        }
      });
    }
  }
};
$(document).ready($.hood.Subscriptions.Init);