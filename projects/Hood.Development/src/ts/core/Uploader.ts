import * as $ from 'jquery';
import { Response } from './Response';
import * as Dropzone from "dropzone";
import { Alerts } from './Alerts';

const dz = Dropzone
dz.autoDiscover = false;

export class Uploader {
    constructor() {
        if ($('.image-uploader').length || $('.gallery-uploader').length) {
            $(".upload-progress-bar").hide();
            $('.image-uploader').each(this.singleImage);
            $('.gallery-uploader').each(this.gallery);
        }
    }

    refreshImage(sender: JQuery<HTMLElement>, data: Response) {
        $(sender.data('preview')).css({
            'background-image': 'url(' + data.media.SmallUrl + ')'
        });
        $(sender.data('preview')).find('img').attr('src', data.media.SmallUrl);
    }

    singleImage(this: HTMLElement) {

        let tag = '#' + $(this).attr('id');
        let $tag = $(tag);

        let jsontag = '#' + $(this).attr('json');

        let avatarDropzone = new Dropzone(tag, {
            url: $tag.data('url'),
            maxFiles: 1,
            paramName: 'file',
            parallelUploads: 1,
            acceptedFiles: $tag.data('types') || ".png,.jpg,.jpeg,.gif",
            autoProcessQueue: true, // Make sure the files aren't queued until manually added
            previewsContainer: false, // Define the container to display the previews
            clickable: tag // Define the element that should be used as click trigger to select files.
        });

        avatarDropzone.on("addedfile", function () {
        });

        avatarDropzone.on("totaluploadprogress", function (progress) {
            $(".upload-progress-bar." + tag.replace('#', '') + " .progress-bar").css({ width: progress + "%" });
        });

        avatarDropzone.on("sending", function (file) {
            $(".upload-progress-bar." + tag.replace('#', '')).show();
            $($tag.data('preview')).addClass('loading');
        });

        avatarDropzone.on("queuecomplete", function (progress) {
            $(".upload-progress-bar." + tag.replace('#', '')).hide();
        });

        avatarDropzone.on("success", function (file, response: Response) {
            if (response.success) {
                if (response.media) {
                    $(jsontag).val(JSON.stringify(response.media));
                    $($tag.data('preview')).css({
                        'background-image': 'url(' + response.media.SmallUrl + ')'
                    });
                    $($tag.data('preview')).find('img').attr('src', response.media.SmallUrl);
                }
                Alerts.success("New image added!");
            } else {
                Alerts.error("There was a problem adding the image: " + response.error);
            }
            avatarDropzone.removeFile(file);
            $($tag.data('preview')).removeClass('loading');
        });
    }

    gallery(this: HTMLElement) {

        let tag = '#' + $(this).attr('id');
        let $tag = $(tag);

        let previewNode = document.querySelector(tag + "-template");
        previewNode.id = "";
        let previewTemplate = (<HTMLElement>previewNode.parentNode).innerHTML;
        previewNode.parentNode.removeChild(previewNode);

        let galleryDropzone = new Dropzone(tag, {
            url: $tag.data('url'),
            thumbnailWidth: 80,
            thumbnailHeight: 80,
            parallelUploads: 5,
            previewTemplate: previewTemplate,
            paramName: 'files',
            acceptedFiles: $tag.data('types') || ".png,.jpg,.jpeg,.gif",
            autoProcessQueue: true, // Make sure the files aren't queued until manually added
            previewsContainer: "#previews", // Define the container to display the previews
            clickable: ".fileinput-button", // Define the element that should be used as click trigger to select files.
            dictDefaultMessage: '<span><i class="fa fa-cloud-upload fa-4x"></i><br />Drag and drop files here, or simply click me!</div>',
            dictResponseError: 'Error while uploading file!'
        });

        $(tag + " .cancel").hide();

        galleryDropzone.on("addedfile", function (file) {
            $(file.previewElement.querySelector(".complete")).hide();
            $(file.previewElement.querySelector(".cancel")).show();
            $(tag + " .cancel").show();
        });

        // Update the total progress bar
        galleryDropzone.on("totaluploadprogress", function (totalProgress: number, totalBytes: number, totalBytesSent: number) {
            let progressBar = document.querySelector("#total-progress .progress-bar") as HTMLElement;
            progressBar.style.width = totalProgress + "%";
        });

        galleryDropzone.on("sending", function (file: Dropzone.DropzoneFile) {
            // Show the total progress bar when upload starts
            let progressBar = document.querySelector("#total-progress") as HTMLElement;
            progressBar.style.opacity = "1";
        });

        // Hide the total progress bar when nothing's uploading anymore
        galleryDropzone.on("complete", function (file) {
            $(file.previewElement.querySelector(".cancel")).hide();
            $(file.previewElement.querySelector(".progress")).hide();
            $(file.previewElement.querySelector(".complete")).show();

            console.error("Uploader.Gallery.Dropzone.OnComplete - Inline.Refresh('.gallery') is not implemented.");
            //Inline.Refresh('.gallery');
        });

        // Hide the total progress bar when nothing's uploading anymore
        galleryDropzone.on("queuecomplete", function (progress) {
            let totalProgress = document.querySelector("#total-progress") as HTMLElement;
            totalProgress.style.opacity = "0";
            $(tag + " .cancel").hide();
        });

        galleryDropzone.on("success", function (file, response: Response) {

            console.error("Uploader.Gallery.Dropzone.OnSuccess - Inline.Refresh('.gallery') is not implemented.");
            //Inline.Refresh('.gallery');

            if (response.success) {
                Alerts.success("New images added!");
            } else {
                Alerts.error("There was a problem adding the profile image: " + response.error);
            }
        });

        // Setup the buttons for all transfers
        // The "add files" button doesn't need to be setup because the config
        // `clickable` has already been specified.
        $(".actions .cancel").click(function () {
            galleryDropzone.removeAllFiles(true);
        });
    }

}