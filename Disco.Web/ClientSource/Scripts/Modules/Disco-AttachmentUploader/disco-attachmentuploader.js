(function (window, document, $) {
    "use strict";

    var attachmentUploader = function ($host) {
        var self = this;

        self.$host = $host;
        self.uploadUrl = $host.attr('data-uploadurl');
        self.dropTarget = $host.find('.Disco-AttachmentUpload-DropTarget');
        self.uploadProgressContainer = $host.find('.Disco-AttachmentUpload-Progress');

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
                title: 'Disco ICT File Uploading'
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
            let mediaStream = null;
            let videoStreaming = false;

            // Setup Dialog
            var dialog = $('<div><video></video></div>')
                .attr({
                    id: 'Disco_AttachmentUpload_ImageDialog',
                    title: 'Upload Image',
                    'class': 'dialog Disco-AttachmentUpload-ImageDialog'
                });
            dialog.dialog({
                autoOpen: true,
                draggable: false,
                modal: true,
                resizable: false,
                width: 720,
                height: 405,
                close: function () {
                    if (mediaStream) {
                        mediaStream.getTracks().forEach(track => track.stop());
                    }
                    window.setTimeout(function () {
                        dialog.dialog('destroy');
                    }, 1);
                }
            }).closest('.ui-dialog').children('.ui-dialog-titlebar').css('border-bottom', 'none');
            const video = dialog.find('video')[0];

            navigator.mediaDevices
                .getUserMedia({
                    audio: false,
                    video: {
                        width: { ideal: 1920 },
                        height: { ideal: 1080 },
                        facingMode: 'environment'
                    }
                })
                .then(stream => {
                    mediaStream = stream;
                    video.srcObject = stream;
                    video.play();
                })
                .catch(err => {
                    console.error(err);
                    dialog.dialog('destroy');
                });

            video.addEventListener('canplay', ev => {
                if (!videoStreaming) {
                    const width = 720;
                    let height = video.videoHeight / (video.videoWidth / width);
                    if (isNaN(height)) {
                        height = 405;
                    }
                    video.setAttribute('width', width);
                    video.setAttribute('height', height);
                    videoStreaming = true;
                    dialog.dialog('option', 'buttons', [{
                        text: 'Capture',
                        click: () => {
                            const canvas = document.createElement('canvas');
                            canvas.width = video.videoWidth;
                            canvas.height = video.videoHeight;
                            const context = canvas.getContext('2d');
                            context.drawImage(video, 0, 0);
                            canvas.toBlob(blob => {
                                self._uploadImage(blob);
                            }, 'image/jpg');
                        }
                    }]);
                    dialog.css('height', '');
                    dialog.closest('.ui-dialog')
                        .children('.ui-dialog-buttonpane')
                        .css('margin-top', 0)
                        .find('.ui-button:first').focus();
                }
            })
        };
        // #endregion

        // #region Online Upload
        self.onlineUpload = async function () {
            const onlineUploadUrl = self.$host.attr('data-onlineuploadurl');
            const qrCodeUrl = self.$host.attr('data-qrcodeurl');
            const $button = self.$host.find('.attachmentInput span.online-upload');

            if ($button.hasClass('fa-spinner'))
                return;

            $button
                .removeClass('fa-qrcode')
                .addClass('fa-spinner fa-spin d-green');

            if (!window.QRCode) {
                const qrCodeElement = document.createElement('script');
                qrCodeElement.src = qrCodeUrl;
                qrCodeElement.type = 'text/javascript';
                qrCodeElement.onload = function () {
                    self.onlineUploadDisplay();
                };
                document.body.appendChild(qrCodeElement);
            }

            const formData = new FormData();
            formData.append('__RequestVerificationToken', self.$host.find('input[name="__RequestVerificationToken"]').val());
            const result = await fetch(onlineUploadUrl, {
                method: 'POST',
                body: formData
            });

            if (!result.ok) {
                alert('Error creating online upload session: ' + result.statusText);
                return;
            }

            const resultModel = await result.json();

            if (!resultModel.Success) {
                alert('Unable to create online upload session: ' + result.ErrorMessage);
                return;
            }

            self.onlineUploadSession = resultModel;
            self.onlineUploadDisplay();

            $button
                .removeClass('fa-spinner fa-spin d-green')
                .addClass('fa-qrcode');
        }
        self.onlineUploadDisplay = function () {
            if (!!window.QRCode && !!self.onlineUploadSession) {
                var dialog = $('<div>')
                    .attr({
                        title: 'Online Upload',
                        'class': 'dialog Disco-AttachmentUpload-OnlineUploadDialog'
                    });
                var qrCode = QRCode({
                    msg: self.onlineUploadSession.SessionUri,
                    ecl: 'L'
                });
                dialog.append(qrCode);
                $('<input type="text" readonly>')
                    .val(self.onlineUploadSession.SessionUri)
                    .appendTo(dialog);
                $('<div class="info-box"><p class="fa-p"><i class="fa fa-info-circle information"></i> Scan the QR Code or send the link to upload files</p></div>')
                    .appendTo(dialog);

                var expiration = new Date(self.onlineUploadSession.Expiration);
                var sessionExpiration = setTimeout(function () {
                    dialog.dialog('close');
                }, expiration.getTime() - new Date().getTime());

                dialog.dialog({
                    resizable: false,
                    width: 500,
                    modal: true,
                    autoOpen: true,
                    close: function () {
                        if (!!sessionExpiration) {
                            window.clearTimeout(sessionExpiration);
                        }
                        dialog.dialog('destroy').remove();
                    }
                });
            }
        }
        // #endregion

        // #region Helpers
        self.getFileComments = function (fileName, thumbnailHandler, complete) {
            var result = false;
            var dialog = $('<div>')
                .attr({
                    title: 'Upload File',
                    'class': 'dialog Disco-AttachmentUpload-CommentDialog'
                });
            dialog.html('<table><tr><th>File Name:</th><td class="filename"></td></tr><tr><th>Comments:</th><td><input class="comments" type="text" placeholder="optional" /></td></tr><tr><td class="thumbnail" colspan="2"><img /></td></tr></table>');

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
                        window.setTimeout(function () {
                            comments.focus();
                        }, 1);
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

        self._uploadImage = function (blob) {
            var fileName = 'CapturedImage-' + moment().format('YYYYMMDD-HHmmss') + '.jpg';

            self.getFileComments(fileName, function (img) {
                const dataUri = URL.createObjectURL(blob);
                img.attr('src', dataUri);
                return true;
            }, function (result, comments) {
                if (!result)
                    return;

                self._uploadFile(blob, fileName, comments);
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

        self._uploadFile = async function (fileData, fileName, comments) {
            const formData = new FormData();

            const progress = $('<div>')
                .append($('<i>').addClass('fa fa-cog fa-spin'))
                .append($('<span>').text('Uploading: ' + fileName))
                .appendTo(self.uploadProgressContainer);

            formData.append('__RequestVerificationToken', self.$host.find('input[name="__RequestVerificationToken"]').val());
            formData.append('comments', comments);
            formData.append('file', fileData, fileName);

            const result = await fetch(self.uploadUrl, {
                method: 'POST',
                body: formData
            });

            progress.slideUp(400, function () {
                progress.remove();
            });

            if (!result.ok) {
                alert('Error Uploading [' + fileName + ']: ' + result.statusText);

                const errorText = await result.text();
                console.error('Failed to upload file [' + fileName + ']: ' + errorText); 
            }
        };
        // #endregion

        return self;
    };

    if (!document.Disco) {
        document.Disco = {};
    }
    document.Disco.AttachmentUploader = attachmentUploader;

}(this, document, $));