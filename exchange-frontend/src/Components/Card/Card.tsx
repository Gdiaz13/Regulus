import React, { type JSX } from 'react';
import "./Card.css"

interface Props {
    companyName: string;
    ticker: string;
    price: number;
};

const Card: React.FC<Props> = ({ 
    companyName,
    ticker,
    price 
    }: Props) : JSX.Element => {
    return (
        <div className ="card">
            <img 
                src="https://www.royalmint.com/globalassets/bullion/images/products/bars/trm-cast-bars/trmcb500gio.jpg"
                alt="Image"
            />
            <div className="details">
                <h2>{companyName} ({ticker})</h2>
                <p>${price}</p>
            </div>
            <p className = "info">
                Lorem ipsum dolor sit amet consectetur adipisicing elit. Quisquam, voluptatibus.
            </p>
        </div>
    )
}

export default Card;