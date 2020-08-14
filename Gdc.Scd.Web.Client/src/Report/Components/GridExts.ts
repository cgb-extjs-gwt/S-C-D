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

export const clipboardConfig = {

    readonly: {
        formats: {
            text: { put: 'noPut' }
        },
        noPut: function () { }
    },

    text: {
        formats: {
            text: {
                get: 'getTextData',
                put: 'putTextDataFixed'
            }
        },

        putTextDataFixed: function (data, format) {

            var cmp = this.getCmp();
            var store = cmp.getStore();

            if (!store) {
                return;
            }

            var values = (Ext.util as any).TSV.decode(data),
                recCount = values.length,
                colCount = recCount ? values[0].length : 0,
                columns = cmp.getHeaderContainer().getVisibleColumns(),
                maxRowIdx = store.getCount() - 1,
                maxColIdx = columns.length - 1,
                selectable = cmp.getSelectable(),
                selection = selectable && selectable.getSelection(),
                row, sourceRowIdx, sourceColIdx, column, record, columnIndex, recordIndex,
                dataObject, destination, dataIndex, startColumnIndex, startRecordIndex;

            if (maxRowIdx < 0 || maxColIdx <= 0) {
                return;
            }

            if (selection) {
                selection.eachCell(function (c) {
                    destination = c;
                    return false;
                });
            }

            startColumnIndex = destination ? destination.columnIndex : 0;
            startRecordIndex = destination ? destination.recordIndex : 0;

            for (sourceRowIdx = 0; sourceRowIdx < recCount; sourceRowIdx++) {
                row = values[sourceRowIdx];
                recordIndex = startRecordIndex + sourceRowIdx;
                // If we are at the end of the destination store, break the row loop.
                if (recordIndex > maxRowIdx) {
                    break;
                }
                record = store.getAt(recordIndex);

                dataObject = {};
                columnIndex = startColumnIndex;
                sourceColIdx = 0;

                // Collect new values in dataObject
                while (sourceColIdx < colCount && columnIndex <= maxColIdx) {
                    column = columns[columnIndex];
                    dataIndex = column.getDataIndex();

                    // we skip ignored columns
                    if (!column.getIgnoreExport()) {
                        // paste the content if the column allows us to do that, otherwise we ignore it
                        if (dataIndex && (format === 'raw' || format === 'text')) {
                            dataObject[dataIndex] = row[sourceColIdx];
                        }
                        sourceColIdx++;
                    }
                    columnIndex++;
                }

                // Update the record in one go.
                record.set(dataObject);
            }


        }

    }

};