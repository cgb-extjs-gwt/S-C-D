const PREFIX = '/api/';

export enum Methods {
    Get = 'GET',
    Post = 'POST',
    Put = 'PUT',
    Delete = 'DELETE'
}

const request = (url: string, method: Methods, params = null, options = {}) => {
    if (params != null) {
        url = Ext.urlAppend(url, Ext.urlEncode(params, true));
    }

    return new Promise<any>((resolve, reject) => {
        Ext.Ajax.request({
            url,
            method,
            success: response => resolve(response),
            failure: response => reject(response),
            ...options
        });
    })
}

const requestMvc = (
    controller: string, 
    action: string, 
    method: Methods, 
    params = null, 
    options = {}
) => {
    const url = `${PREFIX}${controller}/${action}`;

    return request(url, method, params, options);
}

// const paramsToString = (params: {} | {}[], prefix: string = '') => {
//     const strings = [];

//     if (params instanceof Array) {
//         (<{}[]>params).forEach((item, index) => {

//         })
//     }

//     for (const key of Object.keys(params)) {
//         let str: string;

//         const value = params[key];

//         if (typeof value === 'object'){
//             str = paramsToString(params, key);
//         } else if (value != null) {
//             str = `${key}.${value.toString()}`;
//         }

//         strings.push(encodeURIComponent(str));
//     }

//     return strings.join('&');
// }

export const get = <T>(controller: string, action: string, params = null) => {
    return requestMvc(controller, action, Methods.Get, params).then<T>(resp => JSON.parse(resp.responseText));
}

export const post = <T>(controller: string, action: string, data: T, params = null) => {
    return requestMvc(controller, action, Methods.Post, params, { jsonData: data });
}

export const put = <T>(controller: string, action: string, data: T, params = null) => {
    return requestMvc(controller, action, Methods.Put, params, { jsonData: data });
}

export const deleteItem = (controller: string, action: string, params = null) => {
    return requestMvc(controller, action, Methods.Delete, params);
}