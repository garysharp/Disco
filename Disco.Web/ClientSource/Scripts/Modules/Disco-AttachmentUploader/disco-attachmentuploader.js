/// <reference path="webcam.js" />

; (function (window, document, $, Webcam) {
    "use strict";

    var attachmentUploader = function (uploadUrl, dropTarget, uploadProgressContainer) {
        var self = this;

        self.uploadUrl = uploadUrl;
        self.dropTarget = dropTarget;
        self.uploadProgressContainer = uploadProgressContainer;

        // #region File Selection Support
        self._uploadFilesInput = null;
        self.uploadFiles = function () {
            if (!!self._uploadFilesInput) {
                self._uploadFilesInput.remove();
            }
            self._uploadFilesInput = $('<input>');
            self._uploadFilesInput.attr({
                type: 'file',
                multiple: 'multiple',
                title: 'Disco File Uploading'
            })
                .hide()
                .change(function (e) {
                    var files = e.target.files;
                    if (!!files && files.length > 0) {
                        self._uploadFiles(files);
                    }
                    self._uploadFilesInput.remove();
                }).appendTo(self.uploadProgressContainer)
                .click();
        };
        // #endregion

        // #region File Drop Support
        if (!!self.dropTarget) {
            var $document = $(document);
            var dragFinished = false;
            var dragFinishedToken = null;
            $document.on('dragover', function () {
                self.dropTarget.addClass('dragHighlight');
                self.dropTarget.removeClass('dragHover');
                dragFinished = false;
            });
            $document.on('dragleave', function () {
                if (!!dragFinishedToken)
                    window.clearInterval(dragFinishedToken);

                dragFinished = true;
                window.setTimeout(function () {
                    if (dragFinished)
                        self.dropTarget.removeClass('dragHighlight');
                    dragFinishedToken = null;
                }, 200);
            });

            self.dropTarget.on('dragover', function (e) {
                e.stopPropagation();
                e.preventDefault();

                self.dropTarget.addClass('dragHover');

                dragFinished = false;

                e.originalEvent.dataTransfer.dropEffect = 'copy';
            });

            self.dropTarget.on('drop', function (e) {
                e.stopPropagation();
                e.preventDefault();

                dragFinished = true;
                self.dropTarget.removeClass('dragHighlight');

                var files = e.originalEvent.dataTransfer.files;
                self._uploadFiles(files);
            });
        }
        // #endregion

        // #region Webcam Support
        self.uploadImage = function () {
            var mediaWidth = 720;
            var mediaHeight = 540;
            var mediaStream;

            // Setup Dialog
            var dialog = $('<div>')
                .attr({
                    id: 'disco_attachmentUpload_imageDialog',
                    title: 'Upload Image',
                    'class': 'dialog disco-attachmentUpload-imageDialog'
                });
            dialog.dialog({
                autoOpen: true,
                draggable: false,
                modal: true,
                resizable: false,
                width: mediaWidth,
                height: mediaHeight,
                close: function () {
                    Webcam.reset();
                    window.setTimeout(function () {
                        dialog.dialog('destroy');
                    }, 1);
                }
            }).closest('.ui-dialog').children('.ui-dialog-titlebar').css('border-bottom', 'none');

            var dialogButtons = [{
                text: 'Capture',
                click: captureImage
            }];

            // Capturing
            function captureImage() {
                var dataUri = Webcam.snap();
                self._uploadImage(dataUri);
            }
            Webcam.set({
                width: mediaWidth,
                height: mediaHeight,
                dest_width: mediaWidth * 1.5,
                dest_height: mediaHeight * 1.5,
                jpeg_quality: 95
            });
            Webcam.setSWFLocation('/ClientSource/Scripts/Modules/Disco-AttachmentUploader/webcam.swf');
            Webcam.on('error', function (error) {
                alert(error);
                dialog.dialog('close');
            });
            Webcam.on('live', function () {
                dialog.dialog('option', 'buttons', dialogButtons);
                dialog.closest('.ui-dialog')
                    .children('.ui-dialog-buttonpane')
                    .css('margin-top', 0)
                    .find('.ui-button:first').focus();
            });
            Webcam.attach(dialog.attr('id'));
        };
        // #endregion

        // #region Helpers
        self.getFileComments = function (fileName, thumbnailHandler, complete) {
            var result = false;
            var dialog = $('<div>')
                .attr({
                    title: 'Upload File',
                    'class': 'dialog disco-attachmentUpload-commentDialog'
                });
            dialog.html('<table><tr><th>File Name:</th><td class="filename"></td></tr><tr><th>Comments:</th><td><input class="comments" type="text"></input></td></tr><tr><td class="thumbnail" colspan="2"><img /></td></tr></table>');

            if (!!thumbnailHandler) {
                var td = dialog.find('td.thumbnail');
                var img = td.find('img');
                if (thumbnailHandler(img))
                    td.show();
            }

            dialog.find('td.filename').text(fileName).attr('title', fileName);
            var comments = dialog.find('input.comments')
                .keypress(function (e) {
                    if (e.which === 13) {
                        result = true;
                        dialog.dialog("close");
                    }
                });

            dialog.dialog({
                resizable: false,
                width: 400,
                modal: true,
                autoOpen: true,
                buttons: {
                    "Upload": function () {
                        result = true;
                        dialog.dialog("close");
                    },
                    Cancel: function () {
                        dialog.dialog("close");
                    }
                },
                close: function () {
                    var commentsVal = comments.val();
                    dialog.dialog('destroy').remove();
                    complete(result, commentsVal);
                }
            });
        };

        self._uploadImage = function (dataUri) {
            var imageData = dataUri.replace(/^data\:image\/\w+\;base64\,/, '');

            var imageBlob = new Blob([Webcam.base64DecToArr(imageData)], { type: 'image/jpeg' });

            var fileName = 'CapturedImage-' + moment().format('YYYYMMDD-HHmmss') + '.jpg';

            self.getFileComments(fileName, function (img) {
                img.attr('src', dataUri);
                return true;
            }, function (result, comments) {
                if (!result)
                    return;

                self._uploadFile(imageBlob, fileName, comments);
            });
        };

        self._uploadFiles = function (fileList) {
            var files = $.makeArray(fileList);

            var processNextFile = function () {
                if (!files || files.length === 0)
                    return;

                var file = files.shift();
                self.getFileComments(file.name, function (img) {
                    if (!!file.type && file.type.indexOf('image/') === 0) {
                        var reader = new FileReader();
                        reader.onload = function (e) {
                            img.attr('src', e.target.result);
                        };
                        reader.readAsDataURL(file);
                        return true;
                    }
                    return false;
                }, function (result, comments) {
                    if (!result)
                        return;

                    self._uploadFile(file, file.name, comments);

                    processNextFile();
                });
            };
            processNextFile();
        };

        self._uploadFile = function (fileData, fileName, comments) {
            var formData = new FormData();
            var xhr = new XMLHttpRequest();
            var progress = $('<div>')
                .append($('<i>').addClass('fa fa-cog fa-spin'))
                .append($('<span>').text('Uploading: ' + fileName))
                .appendTo(self.uploadProgressContainer);

            formData.append('Comments', comments);
            formData.append('File', fileData, fileName);

            xhr.open("POST", self.uploadUrl, true);
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4 && xhr.status === 200) {
                    if (xhr.status !== 200) {
                        alert('Error Uploading [' + fileName + ']: ' + xhr.responseText);
                    }
                    progress.slideUp(400, function () {
                        progress.remove();
                    });
                }
            };
            xhr.send(formData);
        };
        // #endregion

        return self;
    };

    if (!document.Disco) {
        document.Disco = {};
    }
    document.Disco.AttachmentUploader = attachmentUploader;

}(this, document, $, Webcam));