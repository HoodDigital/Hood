import Swal, { SweetAlertOptions, SweetAlertResult } from 'sweetalert2';
import 'jquery-toast-plugin';

const BootstrapSwal = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-success btn-lg m-1 pl-4 pr-4',
        cancelButton: 'btn btn-danger btn-lg m-1'
    },
    buttonsStyling: false
});

export class Alerts {

    static log(message: string, type: 'message' | 'error' | 'warning' | 'info' = 'message') {
        if (!document.body.classList.contains('dev-mode')) {
            return;
        }
        switch (type) {
            case 'error':
                console.error(message);
                break;
            case 'message':
                console.log(message);
                break;
           case 'warning':
                console.warn(message);
                break;
           case 'info':
                console.info(message);
                break;
      }
    }

    static error(message: string, title: string = null, hideAfter: number = null) {
      
            $.toast({
                heading: title,
                text: message,
                icon: 'error',
                position: 'bottom-left',
                loader: false,
                bgColor: '#d0100b',
                textColor: 'white', 
                hideAfter: hideAfter
            });

    }

    static warning(message: string, title: string = null, hideAfter: number = null) {
      
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

    static message(message: string, title: string = null, hideAfter: number = null) {
      
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

    static success(message: string, title: string = null, hideAfter: number = null) {
      
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

    static alert(message: string, title: string = null, icon: string = 'info', hideAfter: number = 10000) {

        switch (icon) {
            case 'error':
                Alerts.error(message, title, hideAfter);
                break;
            case 'warning':
                Alerts.warning(message, title, hideAfter);
                break;
            case 'info':
                Alerts.message(message, title, hideAfter);
                break;
            case 'success':
                Alerts.success(message, title, hideAfter);
                break;
        }

    }

    static sweetAlert(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {
        BootstrapSwal.fire(options).then(function (result: any) {
            callback(result);
        });
    }

    static confirm(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {

        let baseOptions: SweetAlertOptions = {
            showCancelButton: true,
            footer: '<span class="text-warning"><i class="fa fa-exclamation-triangle"></i> This cannot be undone.</span>',
            title: 'Are you sure?',
            html: 'Are you sure you want to do this?',
            confirmButtonText: 'Ok', 
            cancelButtonText:'Cancel'
        };

        Alerts.sweetAlert({...baseOptions, ...options}, callback);
    }

    static prompt(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {

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

        Alerts.sweetAlert({...baseOptions, ...options}, callback);
    }

    log(message: string, type: 'message' | 'error' | 'warning' | 'info' = 'message') {
        Alerts.log(message, type);
    }

    error(message: string, title: string = null, hideAfter: number = null) {
        Alerts.error(message, title, hideAfter);
    }

    warning(message: string, title: string = null, hideAfter: number = null) {
        Alerts.warning(message, title, hideAfter);
    }

    message(message: string, title: string = null, hideAfter: number = null) {
        Alerts.message(message, title, hideAfter);
    }

    success(message: string, title: string = null, hideAfter: number = null) {
        Alerts.success(message, title, hideAfter);
    }

    alert(message: string, title: string = null, icon: string = 'info', hideAfter: number = 10000) {
        Alerts.alert(message, message, icon, hideAfter);
    }

    sweetAlert(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {
        Alerts.sweetAlert(options, callback);
    }

    confirm(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {
        Alerts.confirm(options, callback);
    }

    prompt(options: SweetAlertOptions, callback: (result: SweetAlertResult<any>) => void) {
        Alerts.prompt(options, callback);
    }

}