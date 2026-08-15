using Plus.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    class TargetedOffersComposer : ServerPacket
    {
        public TargetedOffersComposer() :
            base(ServerPacketHeader.TargetedOfferComposer)
        {
            //default values
            int priceCredits = 24;
            int priceDiamonds = 23;
            int purchaseLimit = 4;
            int expiration = -1;
            String title = "Oferta";
            String description = "Esta oferta ya no esta disponible";
            String imageURL = "/targetedoffers/oferta1.png";
            String iconURL = "/targetedoffers/icon.png";


            priceCredits = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["cost_credits"]);
            priceDiamonds = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["cost_diamonds"]);
            purchaseLimit = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["purchase_limit"]);
            expiration = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["expiration"]);
            title = PlusEnvironment.GetOffers().DBOffer["title"];
            description = PlusEnvironment.GetOffers().DBOffer["description"];
            imageURL = PlusEnvironment.GetOffers().DBOffer["image_url"];
            iconURL = PlusEnvironment.GetOffers().DBOffer["icon_url"];

            if (PlusEnvironment.GetDBConfig().DBData["targeted_offers_enabled"]== "1")
            { 


                base.WriteInteger(1); //id
                base.WriteInteger(1); //unknown
                base.WriteString("ufo_ny2015_offer"); //identifier
                base.WriteString("ufo_ny2015_offer");//unknown string
                base.WriteInteger(priceCredits); //price credits
                base.WriteInteger(priceDiamonds); // price activity points
                base.WriteInteger(5); //activity points type
                base.WriteInteger(purchaseLimit); //purchase limit
                base.WriteInteger(expiration);//expiration
                base.WriteString(title); //title
                base.WriteString(description);  //description
                base.WriteString(imageURL);//imageURL
                base.WriteString(iconURL);
                base.WriteInteger(0);//type
                base.WriteInteger(1);//vector length?
                base.WriteString("");//string in vector

            }
            else
            {
                base.WriteInteger(1); //id
                base.WriteInteger(1); //unknown
                base.WriteString("ufo_ny2015_offer"); //identifier
                base.WriteString("ufo_ny2015_offer");//unknown string
                base.WriteInteger(priceCredits); //price credits
                base.WriteInteger(priceDiamonds); // price activity points
                base.WriteInteger(5); //activity points type
                base.WriteInteger(0); //purchase limit
                base.WriteInteger(expiration);//expiration
                base.WriteString(title); //title
                base.WriteString(description);  //description
                base.WriteString(imageURL);//imageURL
                base.WriteString(iconURL);
                base.WriteInteger(0);//type
                base.WriteInteger(1);//vector length?
                base.WriteString("");//string in vector
            }

        }
    }
}
