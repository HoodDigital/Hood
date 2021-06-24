import { Inline } from "../core/Inline";

export declare interface PropertyImporterResult {

    ftp: FTPImporterStatistics;
    importer: PropertyImporterStatistics;

}

export declare interface FTPImporterStatistics {

    bytesTransferred: number;
    complete: number;
    statusMessage: string;

}

export declare interface PropertyImporterStatistics {

    running: boolean;

    processed: number;
    statusMessage: string;

    toAdd: number;
    toDelete: number;
    toUpdate: number;
    total: number;

    added: number;
    deleted: number;
    updated: number;

    complete: number;

    errors: string[];
    warnings: string[];

}

export class PropertyImporter {
    updateInterval: number;

    constructor() {
        if ($('#import-property-start').length > 0) {
            this.update();
            $('#import-property-start').click(function (this: PropertyImporter) {
                $.ajax({
                    url: $('#import-property-start').data('url'),
                    type: "POST",
                    error: Inline.handleError,
                    success: function (this: PropertyImporter) {
                        this.update();
                    }.bind(this)
                });
            }.bind(this));
            $('#import-property-cancel').click(function (this: PropertyImporter) {
                $.ajax({
                    url: $('#import-property-cancel').data('url'),
                    type: "POST",
                    error: Inline.handleError,
                    success: function (this: PropertyImporter) {
                        this.update();
                    }.bind(this)
                });
            }.bind(this));
        }
    }

    update() {
        $.ajax({
            url: $('#import-property-status').data('url'),
            type: "POST",
            error: Inline.handleError,
            success: function (this: PropertyImporter, result: PropertyImporterResult) {

                if (result.importer.running) {
                    this.showInfo();
                    clearInterval(this.updateInterval);
                    this.updateInterval = window.setTimeout(this.update, 250);
                } else {
                    clearInterval(this.updateInterval);
                    this.hideInfo();
                }

                $('.tp').html(result.importer.total.toString());
                $('#pu').html(result.importer.updated.toString());
                $('#pa').html(result.importer.added.toString());
                $('#pp').html(result.importer.processed.toString());
                $('#pd').html(result.importer.deleted.toString());
                $('#ToAdd').html(result.importer.toAdd.toString());
                $('#ToUpdate').html(result.importer.toUpdate.toString());
                $('#ToDelete').html(result.importer.toDelete.toString());
                $('#pt').html(result.importer.statusMessage.toString());

                let ftpPercentComplete = Math.round(result.ftp.complete * 100) / 100;
                $('#fp').html(ftpPercentComplete.toString());

                $('#ft').html(result.ftp.statusMessage);

                let percentComplete = Math.round(result.importer.complete * 100) / 100;
                $('.pc').html(percentComplete.toString());

                $('#progressbar').css({
                    width: result.importer.complete + "%"
                });

                if (result.importer.errors.length) {
                    let errorHtml = "";
                    for (let i = result.importer.errors.length - 1; i >= 0; i--) {
                        errorHtml += '<div class="text-danger">' + result.importer.errors[i] + '</div>';
                    }
                    $('#import-property-errors').html(errorHtml);
                } else {
                    $('#import-property-errors').html("<div>No errors reported.</div>");
                }

                if (result.importer.warnings.length) {
                    let warningHtml = "";
                    for (let j = result.importer.warnings.length - 1; j >= 0; j--) {
                        warningHtml += '<div class="text-warning">' + result.importer.warnings[j] + '</div>';
                    }
                    $('#import-property-warnings').html(warningHtml);
                } else {
                    $('#import-property-warnings').html("<div>No warnings reported.</div>");
                }

            }.bind(this)
        });
    }

    hideInfo() {
        $('#import-property-start').removeAttr('disabled');
        $('#import-property-cancel').attr('disabled', 'disabled');
        $('#import-property-progress').removeClass('d-block');
        $('#import-property-progress').addClass('d-none');
    }

    showInfo() {
        $('#import-property-cancel').removeAttr('disabled');
        $('#import-property-start').attr('disabled', 'disabled');
        $('#import-property-progress').addClass('d-block');
        $('#import-property-progress').removeClass('d-none');
    }

}
