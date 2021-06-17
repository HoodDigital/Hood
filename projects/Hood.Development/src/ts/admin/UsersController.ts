import { SweetAlertResult } from "sweetalert2";
import swal from 'sweetalert2';

import { Alerts } from "../core/Alerts";
import { Inline } from "../core/Inline";
import { DataList } from "../core/DataList";
import { RandomStringGenerator } from "../core/RandomStringGenerator";
import { Response } from "../core/Response";
import { ModalController } from "../core/Modal";
import { Validator } from "../core/Validator";

export class UsersController {
    element: HTMLElement;
    list: DataList;

    constructor() {
        this.initLists();

        $('body').on('click', '.user-create', this.create.bind(this));
        $('body').on('click', '.user-delete', this.delete.bind(this));
        $('body').on('change', '#user-create-form #GeneratePassword', this.generatePassword);

        $('body').on('change', '.user-role-check', this.toggleUserRole);

        $('body').on('click', '.user-reset-password', this.resetPassword);

        $('body').on('click', '.user-notes-add', this.addNote.bind(this));
        $('body').on('click', '.user-notes-delete', this.deleteNote.bind(this));

    }

    initLists(this: UsersController): void {

        this.element = document.getElementById('user-list');
        if (this.element) {
            this.list = new DataList(this.element, {
                onComplete: function (this: UsersController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading users list.', 'info');

                }.bind(this)
            });
        }

        this.notesEl = document.getElementById('user-notes');
        if (this.notesEl) {
            this.notesList = new DataList(this.notesEl, {
                onComplete: function (this: UsersController, data: string, sender: HTMLElement = null) {

                    Alerts.log('Finished loading user notes list.', 'info');

                }.bind(this)
            });
        }

    }

    create(this: UsersController, e: JQuery.ClickEvent) {

        e.preventDefault();
        e.stopPropagation();
        let createUserModal: ModalController = new ModalController({
            onComplete: function (this: UsersController) {
                let form = document.getElementById('user-create-form') as HTMLFormElement;
                new Validator(form, {
                    onComplete: function (this: UsersController, response: Response) {

                        Response.process(response, 5000);

                        if (this.list) {
                            this.list.reload();
                        }

                        if (response.success) {
                            createUserModal.close();
                        }

                    }.bind(this)
                });
            }.bind(this)
        });
        createUserModal.show($(e.currentTarget).attr('href'), this.element);
    }

    delete(this: UsersController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The user will be permanently removed."
        }, function (this: UsersController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: UsersController, data: Response) {

                    Response.process(data, 5000);

                    if (this.list) {
                        this.list.reload();
                    }

                    if (e.currentTarget.dataset.redirect) {

                        Alerts.message('Just taking you back to the content list.', 'Redirecting...');
                        setTimeout(function () {
                            window.location = e.currentTarget.dataset.redirect;
                        }, 1500);

                    }

                }.bind(this));
            }
        }.bind(this))

    }

    generatePassword() {
        if ($(this).is(':checked')) {
            let generator = new RandomStringGenerator({ numSpecial: 1 });
            $('#user-create-form #Password').val(generator.generate(8));
            $('#user-create-form #Password').attr('type', 'text');
        } else {
            $('#user-create-form #Password').val('');
            $('#user-create-form #Password').attr('type', 'password');
        }
    }

    toggleUserRole() {
        if ($(this).is(':checked')) {
            $.post($(this).data('url'), { role: $(this).val(), add: true }, function (data: Response) {
                Response.process(data);
            });
        } else {
            $.post($(this).data('url'), { role: $(this).val(), add: false }, function (data: Response) {
                Response.process(data);
            });
        }
    }

    resetPassword(this: UsersController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.prompt({
            // Confirm options...
            title: "Reset Password",
            html: "Please enter a new password for the user...",
            preConfirm: (inputValue) => {
                if (inputValue === false) return false;
                if (inputValue === "") {
                    swal.showValidationMessage("You didn't supply a new password, we can't reset the password without it!");
                    return false;
                }
            }
        }, function (this: UsersController, result: SweetAlertResult) {

            if (result.isDismissed) {
                return false;
            }

            $.post(e.currentTarget.href, { password: result.value }, function (data: Response) {
                Response.process(data, 5000);
            });

        }.bind(this))

    }

    notesEl: HTMLElement;
    notesList: DataList;

    addNote(this: UsersController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.prompt({
            // Confirm options...
            title: "Add a note",
            html: "Enter and store a note about this user.",
            input: 'textarea',
            preConfirm: (inputValue) => {
                if (inputValue === false) return false;
                if (inputValue === "") {
                    swal.showValidationMessage("You didn't enter a note!");
                    return false;
                }
            }
        }, function (this: UsersController, result: SweetAlertResult) {

            if (result.isDismissed) {
                return false;
            }

            $.post(e.currentTarget.href, { note: result.value }, function (this: UsersController, data: Response) {
                Response.process(data, 5000);


                if (this.notesList) {
                    this.notesList.reload();
                }

            }.bind(this));

        }.bind(this))

    }

    deleteNote(this: UsersController, e: JQuery.ClickEvent) {
        e.preventDefault();
        e.stopPropagation();

        Alerts.confirm({
            // Confirm options...
            title: "Are you sure?",
            html: "The note will be permanently removed."
        }, function (this: UsersController, result: SweetAlertResult) {
            if (result.isConfirmed) {
                Inline.post(e.currentTarget.href, e.currentTarget, function (this: UsersController, data: Response) {

                    Response.process(data, 5000);

                    if (this.notesList) {
                        this.notesList.reload();
                    }

                }.bind(this));
            }
        }.bind(this))

    }

} 