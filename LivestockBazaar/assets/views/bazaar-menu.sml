<lane layout={MenuSize} orientation="horizontal">
  <include *if={DisplayOwner} name="mushymato.LivestockBazaar/views/bazaar-owner" />
  <include name="mushymato.LivestockBazaar/views/bazaar-forsale" />
  <spacer *if={DisplayOwner} layout="296px 0px"/>
</lane>
