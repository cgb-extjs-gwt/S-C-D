import { AutoColumnType } from "../Model/AutoColumnType";
import { AutoFilterType } from "../Model/AutoFilterType";

export const fakeSchema = {

    id: '1',

    name: 'fake-report',

    caption: "Fake report",

    fields: [
        { name: 'col_1', text: 'Super fields 1', type: AutoColumnType.NUMBER },
        { name: 'col_2', text: 'Super fields 2', type: AutoColumnType.TEXT },
        { name: 'col_3', text: 'Super fields 3', type: AutoColumnType.TEXT },
        { name: 'col_4', text: 'Super fields 4', type: AutoColumnType.TEXT }
    ],

    filter: [
        { name: 'col_1', text: 'Super fields 1', type: AutoFilterType.NUMBER },
        { name: 'col_2', text: 'Super fields 2', type: AutoFilterType.TEXT },
        { name: 'col_4', text: 'Super fields 4', type: AutoFilterType.TEXT }
    ]

};