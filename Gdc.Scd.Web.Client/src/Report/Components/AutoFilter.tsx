import { Button, Container, NumberField, Panel, PanelProps, TextField } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { CountryGroupField } from "../../Dict/Components/CountryGroupField";
import { DurationField } from "../../Dict/Components/DurationField";
import { ProActiveField } from "../../Dict/Components/ProActiveField";
import { ReactionTimeField } from "../../Dict/Components/ReactionTimeField";
import { ReactionTypeField } from "../../Dict/Components/ReactionTypeField";
import { SelectField } from "../../Dict/Components/SelectField";
import { ServiceLocationField } from "../../Dict/Components/ServiceLocationField";
import { SogField } from "../../Dict/Components/SogField";
import { StandardWgField } from "../../Dict/Components/StandardWgField";
import { SwDigitField } from "../../Dict/Components/SwDigitField";
import { UserCountryField } from "../../Dict/Components/UserCountryField";
import { WgAllField } from "../../Dict/Components/WgAllField";
import { WgField } from "../../Dict/Components/WgField";
import { YearField } from "../../Dict/Components/YearField";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { AutoFilterModel } from "../Model/AutoFilterModel";
import { AutoFilterType } from "../Model/AutoFilterType";
import { SwDigitSogField } from "../../Dict/Components/SwDigitSogField";

export interface AutoFilterPanelProps extends PanelProps {
    filter: AutoFilterModel[];
    onSearch(filter: any): void;
}

export class AutoFilter extends React.Component<AutoFilterPanelProps, any> {

    private dictSrv: IDictService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        var filter = this.props.filter || [];

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Container
                    ref="fields"
                    margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        clearable: 'true'
                    }}>

                    {filter.map(this.createField)}

                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    private createField(model: AutoFilterModel, index: number): JSX.Element {

        if (model.multiSelect) {
            return this.createMultiSelectField(model, index);
        }

        switch (model.type) {

            case AutoFilterType.NUMBER:
                return <NumberField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.WG:
                return <WgField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.WGALL:
                return <WgAllField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.WGSTANDARD:
                return <StandardWgField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.SOG:
                return <SogField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.COUNTRY:
                return <CountryField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.USERCOUNTRY:
                return <UserCountryField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.COUNTRYGROUP:
                return <CountryGroupField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.AVAILABILITY:
                return <AvailabilityField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.DURATION:
                return <DurationField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.YEAR:
                return <YearField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.REACTIONTIME:
                return <ReactionTimeField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.REACTIONTYPE:
                return <ReactionTypeField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.PROACTIVE:
                return <ProActiveField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.SERVICELOCATION:
                return <ServiceLocationField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.SWDIGIT:
                return <SwDigitField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.SWDIGITSOG:
                return <SwDigitSogField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;

            case AutoFilterType.TEXT:
            default:
                return <TextField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;
        }
    }

    private createMultiSelectField(model: AutoFilterModel, index: number): JSX.Element {
        let store = null;
        switch (model.type) {

            case AutoFilterType.WG: store = this.dictSrv.getWG; break;

            case AutoFilterType.WGALL: store = this.dictSrv.getWgWithMultivendor; break;

            case AutoFilterType.WGSTANDARD: store = this.dictSrv.getStandardWg; break;

            case AutoFilterType.SOG: store = this.dictSrv.getSog; break;

            case AutoFilterType.COUNTRY: store = () => this.dictSrv.getMasterCountries(true); break;

            case AutoFilterType.USERCOUNTRY: store = () => this.dictSrv.getUserCountries(true); break;

            case AutoFilterType.COUNTRYGROUP: store = this.dictSrv.getCountryGroups; break;

            case AutoFilterType.AVAILABILITY: store = this.dictSrv.getAvailabilityTypes; break;

            case AutoFilterType.DURATION: store = this.dictSrv.getDurationTypes; break;

            case AutoFilterType.YEAR: store = this.dictSrv.getYears; break;

            case AutoFilterType.REACTIONTIME: store = this.dictSrv.getReactionTimeTypes; break;

            case AutoFilterType.REACTIONTYPE: store = this.dictSrv.getReactionTypes; break;

            case AutoFilterType.PROACTIVE: store = this.dictSrv.getProActive; break;

            case AutoFilterType.SERVICELOCATION: store = this.dictSrv.getServiceLocationTypes; break;

            case AutoFilterType.SWDIGIT: store = this.dictSrv.getSwDigit; break;

            case AutoFilterType.SWDIGITSOG: store = this.dictSrv.getSwDigitSog; break;

            default: return null;
        }

        return <SelectField key={index} ref={model.name} store={store} multiSelect={true} name={model.name} label={model.text} value={model.value} />;
    }

    public getModel(): any {
        let result = {};
        let filter = this.props.filter;

        if (filter) {
            for (let i = 0, item; item = filter[i]; i++) {

                let f = this.refs[item.name] as any;

                if (f.getValue()) {
                    result[item.name] = f.getValue();
                }
            }
        }

        return result;
    }

    private init() {
        this.createField = this.createField.bind(this);
        this.createMultiSelectField = this.createMultiSelectField.bind(this);
        this.dictSrv = DictFactory.getDictService();
        this.onSearch = this.onSearch.bind(this);
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }
}