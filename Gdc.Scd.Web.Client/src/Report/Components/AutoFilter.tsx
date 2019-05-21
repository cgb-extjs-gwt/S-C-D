import { Button, Container, NumberField, Panel, PanelProps, TextField } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { CountryGroupField } from "../../Dict/Components/CountryGroupField";
import { DurationField } from "../../Dict/Components/DurationField";
import { HardwareWgField } from "../../Dict/Components/HardwareWgField";
import { fillWgSogInfo } from "../../Dict/Components/MultiSelectWg";
import { ProActiveField } from "../../Dict/Components/ProActiveField";
import { ReactionTimeField } from "../../Dict/Components/ReactionTimeField";
import { ReactionTypeField } from "../../Dict/Components/ReactionTypeField";
import { SelectField } from "../../Dict/Components/SelectField";
import { ServiceLocationField } from "../../Dict/Components/ServiceLocationField";
import { SogField, fillSogInfo } from "../../Dict/Components/SogField";
import { StandardWgField } from "../../Dict/Components/StandardWgField";
import { SwDigitField } from "../../Dict/Components/SwDigitField";
import { SwDigitSogField } from "../../Dict/Components/SwDigitSogField";
import { UserCountryField } from "../../Dict/Components/UserCountryField";
import { WgAllField } from "../../Dict/Components/WgAllField";
import { WgField } from "../../Dict/Components/WgField";
import { WgSogField } from "../../Dict/Components/WgSogField";
import { YearField } from "../../Dict/Components/YearField";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { AutoFilterModel } from "../Model/AutoFilterModel";
import { AutoFilterType } from "../Model/AutoFilterType";

export interface AutoFilterPanelProps extends PanelProps {
    filter: AutoFilterModel[];
    onSearch(filter: any): void;
}

export class AutoFilter extends React.Component<AutoFilterPanelProps, any> {

    private dictSrv: IDictService;

    public state: any = {};

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
                return <WgField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} sog={this.state.sog} itemTpl={fillWgSogInfo} />;

            case AutoFilterType.WGALL:
                return <WgAllField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} itemTpl={fillWgSogInfo}/>;

            case AutoFilterType.WGSTANDARD:
                return <StandardWgField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} itemTpl={fillWgSogInfo}/>;

            case AutoFilterType.WGHARDWARE:
                return <HardwareWgField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} itemTpl={fillWgSogInfo}/>;

            case AutoFilterType.WGSOG:
                return <WgSogField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} itemTpl={fillWgSogInfo}/>;

            case AutoFilterType.SOG:
                return <SogField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} onChange={this.onSogChange} itemTpl={fillSogInfo} />;

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
                return <SwDigitField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} sog={this.state.sog} />;

            case AutoFilterType.SWDIGITSOG:
                return <SwDigitSogField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} onChange={this.onSogChange} />;

            case AutoFilterType.TEXT:
            default:
                return <TextField key={index} ref={model.name} name={model.name} label={model.text} value={model.value} />;
        }
    }

    private createMultiSelectField(model: AutoFilterModel, index: number): JSX.Element {
        let cfg: any = {};
        switch (model.type) {

            case AutoFilterType.WG:
                cfg.store = this.dictSrv.getWG;
                cfg.filter = { name: 'sogId', id: this.state.sog };
                cfg.itemTpl = fillWgSogInfo;
                break;

            case AutoFilterType.WGALL:
                cfg.store = this.dictSrv.getWgWithMultivendor;
                cfg.itemTpl = fillWgSogInfo;
                break;

            case AutoFilterType.WGSTANDARD:
                cfg.store = this.dictSrv.getStandardWg;
                cfg.itemTpl = fillWgSogInfo;
                break;

            case AutoFilterType.WGHARDWARE:
                cfg.store = this.dictSrv.getHardwareWg;
                cfg.itemTpl = fillWgSogInfo;
                break;

            case AutoFilterType.WGSOG:
                cfg.store = this.dictSrv.getWgWithSog;
                cfg.itemTpl = fillWgSogInfo;
                break;

            case AutoFilterType.SOG:
                cfg.store = this.dictSrv.getSog;
                cfg.itemTpl = fillSogInfo;
                break;

            case AutoFilterType.COUNTRY: cfg.store = () => this.dictSrv.getMasterCountries(true); break;

            case AutoFilterType.USERCOUNTRY: cfg.store = () => this.dictSrv.getUserCountries(true); break;

            case AutoFilterType.COUNTRYGROUP: cfg.store = this.dictSrv.getCountryGroups; break;

            case AutoFilterType.AVAILABILITY: cfg.store = this.dictSrv.getAvailabilityTypes; break;

            case AutoFilterType.DURATION: cfg.store = this.dictSrv.getDurationTypes; break;

            case AutoFilterType.YEAR: cfg.store = this.dictSrv.getYears; break;

            case AutoFilterType.REACTIONTIME: cfg.store = this.dictSrv.getReactionTimeTypes; break;

            case AutoFilterType.REACTIONTYPE: cfg.store = this.dictSrv.getReactionTypes; break;

            case AutoFilterType.PROACTIVE: cfg.store = this.dictSrv.getProActive; break;

            case AutoFilterType.SERVICELOCATION: cfg.store = this.dictSrv.getServiceLocationTypes; break;

            case AutoFilterType.SWDIGIT:
                cfg.store = this.dictSrv.getSwDigit
                cfg.filter = { name: 'sogId', id: this.state.sog };
                break;

            case AutoFilterType.SWDIGITSOG: cfg.store = this.dictSrv.getSwDigitSog; break;

            default: return null;
        }

        return <SelectField key={index} ref={model.name} {...cfg} multiSelect={true} name={model.name} label={model.text} value={model.value} />;
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
        //
        this.onSogChange = this.onSogChange.bind(this);
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private onSogChange(view: any, newValue: any, oldValue: any) {
        this.setState({ sog: newValue });
    }
}