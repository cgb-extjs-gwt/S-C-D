import { buildMvcUrl } from "../Common/Services/Ajax";
import { WebworkerHelper } from "../Common/Helpers/WebworkerHelper";

const _baseurl = buildMvcUrl('notify', 'connect');

/** 
 * HTML5 web workder task
 * implement infinite long polling connection
*/
function connect() {
    let last_index = 0;
    //let url = '_baseurl?_dc=' + new Date().getTime();
    let url = 'http://localhost:11166/scd/api/notify/connect?_dc=' + new Date().getTime();

    try {


        let xhr = new XMLHttpRequest();
        xhr.open('get', url, true);
        xhr.onprogress = function () {

            //get new string block
            //and parse json

            let curr_index = xhr.responseText.length;
            if (last_index == curr_index) {
                return;
            }

            let s = xhr.responseText.substring(last_index, curr_index);
            last_index = curr_index;

            try {
                let data = JSON.parse(s);
                self.postMessage(data, null);
            }
            catch (e) {
                console.log(e, s);
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
};

function fakeConnect() {
    setInterval(function () {
        self.postMessage({ cur_time: new Date().getTime() }, null);
    }, 2500);
}

let instance: Worker;

export function notifyWorker() {
    if (!instance) {
        var fn = connect.toString();
        fn = fn.replace('_baseurl', _baseurl);
        instance = WebworkerHelper.run(fn);
    }
    return instance;
};
