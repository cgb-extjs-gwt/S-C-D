import { CostElementInput } from "../States/CostElementState";
import { CostBlockInput } from "../States/CostBlock";
import { NamedId } from "../../Common/States/NamedId";

// export interface CostElementInputDto {
//     applications: NamedId[]
//     scopes: 
// }

export const get = () => Promise.resolve(<CostElementInput>{
    applications: {
        list: [
            { id: 'hardware', name: 'Hardware' },
            { id: 'software', name: 'Software' }
        ]
    },
    scopes: {
        list: [
            { id: 'centralcostelements', name: 'Central cost elements' },
            { id: 'localcostelements', name: 'Local cost elements' }
        ]
    },
    countries: [
        { id: 'Algeria', name: 'Algeria' },
        { id: 'Austria', name: 'Austria' },
        { id: 'Balkans', name: 'Balkans' },
        { id: 'Belgium', name: 'Belgium' },
        { id: 'CIS & Russia', name: 'CIS & Russia' },
        { id: 'Czech Republic', name: 'Czech Republic' },
        { id: 'Denmark', name: 'Denmark' },
        { id: 'Egypt', name: 'Egypt' },
        { id: 'Finland', name: 'Finland' },
        { id: 'France', name: 'France' },
        { id: 'Germany', name: 'Germany' },
        { id: 'Greece', name: 'Greece' },
        { id: 'Hungary', name: 'Hungary' },
        { id: 'India', name: 'India' },
        { id: 'Italy', name: 'Italy' },
        { id: 'Luxembourg', name: 'Luxembourg' },
        { id: 'Middle East', name: 'Middle East' },
        { id: 'Morocco', name: 'Morocco' },
        { id: 'Netherlands', name: 'Netherlands' },
        { id: 'Norway', name: 'Norway' },
        { id: 'Poland', name: 'Poland' },
        { id: 'Portugal', name: 'Portugal' },
        { id: 'South Africa', name: 'South Africa' },
        { id: 'Spain', name: 'Spain' },
        { id: 'Sweden', name: 'Sweden' },
        { id: 'Switzerland', name: 'Switzerland' },
        { id: 'Tunisia', name: 'Tunisia' },
        { id: 'Turkey', name: 'Turkey' },
        { id: 'UK & Ireland', name: 'UK & Ireland' },

    ],
    costBlocks: {
        list: [
            { id: '1', name: 'Cost Block 1'},
            { id: '2', name: 'Cost Block 2'},
            { id: '3', name: 'Cost Block 3'},
            { id: '4', name: 'Cost Block 4'},
            { id: '5', name: 'Cost Block 5'},
            { id: '6', name: 'Cost Block 6'},
            { id: '7', name: 'Cost Block 7'},
            { id: '8', name: 'Cost Block 8'},
            { id: '9', name: 'Cost Block 9'},
            { id: '10', name: 'Cost Block 10'},
        ]
    }
});

export const getCostBlock = (costBlockId: string) => Promise.resolve(<CostBlockInput>{
    id: costBlockId,
    name: `Cost Block ${costBlockId}`,
    costElements: {
        list: []
    }
});
