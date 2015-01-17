/*global angular */
'use strict';

/**
 * The main app module
 * @name app
 * @type {angular.Module}
 */
var app =
    angular.
        module('app', ['flow']);

app.config(['flowFactoryProvider', appConfig]);


function appConfig(flowFactoryProvider) {

    var myCustomData = {
        requestVerificationToken: 'xsrf',
        blueElephant: '42'
    };

    flowFactoryProvider.defaults = {
        target: 'api/upload',
        permanentErrors: [404, 500, 501],
        maxChunkRetries: 1,
        chunkRetryInterval: 5000,
        simultaneousUploads: 4,
        query: myCustomData
    };
    flowFactoryProvider.on('catchAll', function (event) {
        console.log('catchAll', arguments);
    });
    // Can be used with different implementations of Flow.js
    // flowFactoryProvider.factory = fustyFlowFactory;
}



