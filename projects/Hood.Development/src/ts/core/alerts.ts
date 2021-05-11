import Swal, { SweetAlertIcon, SweetAlertInput, SweetAlertOptions, SweetAlertResult } from 'sweetalert2';
import 'jquery-toast-plugin';

const BootstrapSwal = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-success btn-lg m-1 pl-4 pr-4',
        cancelButton: 'btn btn-danger btn-lg m-1'
    },
    buttonsStyling: false
});

export class Alerts {

    static Error(message: string, title: string = null, hideAfter: number = null) {
      
            $.toast({
                heading: title,
                text: message,
                icon: 'error',
                position: 'bottom-left',
                loader: false,
                bgColor: '#d0100c',
                textColor: 'white',
                hideAfter: hideAfter
            });

    }

    static Warning(message: string, title: string = null, hideAfter: number = null) {
      
        $.toast({
            heading: title,
            text: message,
            icon: 'error',
            position: 'bottom-left',
            loader: false,
            bgColor: '#ef9007',
            textColor: 'white',
            hideAfter: hideAfter
        });

    }       

    static Message(message: string, title: string = null, hideAfter: number = null) {
      
        $.toast({
            heading: title,
            text: message,
            icon: 'error',
            position: 'bottom-left',
            loader: false,
            bgColor: '#222222',
            textColor: 'white',
            hideAfter: hideAfter
        });

    }     

    static Success(message: string, title: string = null, hideAfter: number = null) {
      
        $.toast({
            heading: title,
            text: message,
            icon: 'error',
            position: 'bottom-left',
            loader: false,
            bgColor: '#28a745',
            textColor: 'white',
            hideAfter: hideAfter
        });

    }   

    static Alert(message: string, title: string = null, icon: string = 'info', hideAfter: number = 10000) {

        switch (icon) {
            case 'error':
                Alerts.Error(message, title, hideAfter);
                break;
            case 'warning':
                Alerts.Warning(message, title, hideAfter);
                break;
            case 'info':
                Alerts.Message(message, title, hideAfter);
                break;
            case 'success':
                Alerts.Success(message, title, hideAfter);
                break;
        }

    }

    static SweetAlert(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {
        BootstrapSwal.fire(options).then(function (result: any) {
            callback(result);
        });
    }

    static Confirm(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {

        let baseOptions: SweetAlertOptions = {
            showCancelButton: true,
            footer: '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>', 
            confirmButtonText: 'Ok', 
            cancelButtonText:'Cancel'
        };

        Alerts.SweetAlert({...baseOptions, ...options}, callback);
    }

    static Prompt(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {

        let baseOptions: SweetAlertOptions = {
            input: 'text',
            inputAttributes: {
                autocapitalize: 'off'
            },
            showCancelButton: true,
            icon: 'info',
            footer: '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>', 
            confirmButtonText: 'Ok', 
            cancelButtonText: 'Cancel'
        };

        Alerts.SweetAlert({...baseOptions, ...options}, callback);
    }
}