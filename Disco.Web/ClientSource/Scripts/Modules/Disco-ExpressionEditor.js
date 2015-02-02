/// <reference path="../../Core/jquery-1.7.1.js" />
function DiscoExpressionEditor(host, validateUrl, expression) {
    this.host = host;
    this.hostDocument = null;
    this.hostContainer = null;
    this.validateUrl = validateUrl;
    if (expression)
        this.expression = expression;
    else
        this.expression = '';
    this.expressionHtml = '';
    this.expressionException = null;

    // Events
    this.hostInited = null;
    this.expressionValidated = null;
    this.expressionExceptionChanged = null;
}
DiscoExpressionEditor.prototype = {
    hostInit: function () {
        var that = this;
        var hostInited = function () {
            that.hostDocument = that.host.contents();
            that.hostContainer = that.hostDocument.find('body');

            that.host.focus(function () {
                that.setException(null);
                that.renderExpression();
            });

            that.hostContainer.bind('paste', function (el) {
                setTimeout(function () { that.setExpression(that.hostContainer.text()); }, 50);
            });

            if (that.expression)
                that.setExpression(that.expression);

            if (that.hostInited)
                that.hostInited();
        }
        var designModeInit = function () {
            that.host.unbind('load', designModeInit);
            that.host.load(hostInited);
            that.host[0].contentWindow.document.designMode = 'on';
        }
        that.host.load(designModeInit);
    },
    parseExpression: function (expression, exception) {
        var expressionLines = expression.split('\n');
        for (var i = 0; i < expressionLines.length; i++) {
            if (exception && (exception.PositionRow == i + 1)) {
                // Exception Row
                var lineSrc = expressionLines[i].trim();
                var line = '<p id="line' + i + '" class="line lineError">';
                if (lineSrc.length >= exception.PositionColumn) {
                    line += lineSrc.substr(0, exception.PositionColumn - 1);
                    line += '<span class="error">' + lineSrc.substr(exception.PositionColumn - 1, 1) + '</span>';
                    line += lineSrc.substr(exception.PositionColumn);
                } else {
                    line += lineSrc;
                    line += '<span class="error">&nbsp;</span>';
                }
                line += '</p>';
                expressionLines[i] = line;
            } else {
                expressionLines[i] = '<p id="line' + i + '" class="line">' + expressionLines[i].trim() + '</p>';
            }
        }
        return expressionLines.join('');
    },
    setExpression: function (expression) {
        this.expression = expression;
        this.setException(null);
        this.renderExpression();
    },
    getExpression: function () {
        var e = null;
        $('p', this.hostContainer).each(function () {
            if (e == null)
                e = $(this).text();
            else
                e += '\n' + $(this).text();
        });
        this.expression = e;
        return e;
    },
    setException: function (exception) {
        if (this.expressionException !== exception) {
            this.expressionException = exception;
            if (this.expressionExceptionChanged)
                this.expressionExceptionChanged(exception);
        }
    },
    renderExpression: function () {
        this.expressionHtml = this.parseExpression(this.expression, this.expressionException);
        this.hostContainer.html(this.expressionHtml);
    },
    validateExpression: function () {
        var that = this;
        var e = that.getExpression();
        $.getJSON(that.validateUrl, { Expression: e }, function (response, result) {
            that.setException(response);
            that.renderExpression();

            if (that.expressionValidated)
                that.expressionValidated(response.ExpressionValid, response);
        })
    }
}
String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, "");
}
