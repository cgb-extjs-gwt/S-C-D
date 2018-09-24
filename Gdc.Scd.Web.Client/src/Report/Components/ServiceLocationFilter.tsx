import { DictFilter, DictFilterProps } from "./DictFilter";

export class ServiceLocationFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getServiceLocationTypes()
        });
    }
}