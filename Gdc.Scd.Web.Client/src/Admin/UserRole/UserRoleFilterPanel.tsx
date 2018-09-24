import * as React from "react";
import { ComboBoxField, CheckBoxField, Container, Button, Panel, PanelProps } from "@extjs/ext-react";
import { UserRoleService } from "./UserRoleService";
import { UserRoleFilterModel } from "./UserRoleFilterModel";

export interface UserRoleFilterPanelProps extends PanelProps {
    onSearch(filter: UserRoleFilterModel): void;
}

export class UserRoleFilterPanel extends React.Component<UserRoleFilterPanelProps, any> {

    private user: ComboBoxField;

    private country: ComboBoxField;

    private role: ComboBoxField;

    private srv: UserRoleService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'
                    }}
                >
                    <ComboBoxField ref="user" label="User:" options={this.state.users} />
                    <ComboBoxField ref="role" label="Role:" options={this.state.roles} />
                    <ComboBoxField ref="country" label="Country:" options={this.state.countries} />
                </Container>

                <Button text="Search" ui="action" width="100px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        Promise.all([
            this.srv.getUsers(),
            this.srv.getRoles(),
            this.srv.getCountries()
        ]).then(x => {
            this.setState({
                users: x[0],
                roles:x[1],
                countries: x[2]
            });
        });
        //
        this.user = this.refs.user as ComboBoxField;
        this.role = this.refs.role as ComboBoxField;
        this.country = this.refs.country as ComboBoxField;
    }

    public getModel(): UserRoleFilterModel {
        return {
            user: this.getSelected(this.user),
            role: this.getSelected(this.role),
            country: this.getSelected(this.country)
        };
    }

    private init() {
        this.srv = new UserRoleService();
        //
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            users: [],
            roles: [],
            countries: []
        };
    }


    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private getSelected(cb: ComboBoxField): string {
        let result: string = null;
        let selected = (cb as any).getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }
}