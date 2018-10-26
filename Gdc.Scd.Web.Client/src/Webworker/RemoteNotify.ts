import { WebworkerHelper } from "../Common/Helpers/WebworkerHelper";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { APP_REMOTE_DEFAULT } from "../Layout/Actions/NotifyActions";

/** 
 * HTML5 web workder task
 * implement infinite long polling connection
*/
const connect = (function () {

    const _baseurl = window.location.protocol + '//' + window.location.host + buildMvcUrl('notify', 'connect');

    //return function src as string!!! and replace server '_baseurl'

    return function connect() {
        let last_index = 0;
        let url = '_baseurl?_dc=' + new Date().getTime();
        try {
            let xhr = new XMLHttpRequest();
            xhr.open('get', url, true);
            xhr.onprogress = function () {

                let curr_index = xhr.responseText.length;
                if (last_index == curr_index) {
                    return;
                }

                //get new string block

                let batch = xhr.responseText.substring(last_index, curr_index);
                last_index = curr_index;

                //important to split server answer to get valid json packages
                let messages = batch.split('\n---\n');

                //ok, parse json

                for (let i = 0, len = messages.length; i < len; i++) {

                    let s = messages[i];

                    if (!s) {
                        continue;
                    }

                    try {
                        let data = JSON.parse(s);

                        //ignore 'hello' server answer
                        //post only significant data

                        if (data.type !== '<HELLO>') {
                            self.postMessage(data, null);
                        }
                    }
                    catch (e) {
                        console.log(e, s);
                    }

                }
            };
            xhr.onreadystatechange = function (e) {

                if (xhr.readyState != 4) {
                    return;
                }

                //request finished, reconnect

                if (xhr.status == 200) {
                    connect();
                }
                else {
                    setTimeout(connect, 60000); //1 min wait before try reconnect...
                }
            };
            xhr.send();
        }
        catch (e) {
            console.log(e);
        }
    }.toString().replace('_baseurl', _baseurl);

})();

function fakeConnect() {
    setInterval(function () {
        self.postMessage({ type: 'APP.REMOTE.REPORT', text: new Date().getTime() }, null);
    }, 5000);
}

let instance: Worker;

export function RemoteNotify(dispatch) {
    if (!instance) {
        //instance = WebworkerHelper.run(connect);
        instance = WebworkerHelper.run(fakeConnect);

        //listen to worker answer and dispatch event to main window
        instance.onmessage = function (e: MessageEvent) {
            let d = e.data;
            if (d.type) {
                d.type = d.type.toUpperCase();
            }
            else {
                d.type = APP_REMOTE_DEFAULT; //unknown message prepare as default
            }
            dispatch(d);
        };
    }
    return instance;
};
