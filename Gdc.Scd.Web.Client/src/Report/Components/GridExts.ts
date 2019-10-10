export function readonly(field) {
    return function (d) {
        return d[field];
    };
};

export function setFloatOrEmpty(record, field) {
    var d = parseFloat(record.get(field));
    //stub, for correct null imput
    var v = isNaN(d) ? '' : d;
    record.set(field, v);
};