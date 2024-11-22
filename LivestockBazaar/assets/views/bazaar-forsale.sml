<frame layout={:ForSaleLayout}
       background={@Mods/StardewUI/Sprites/MenuBackground}
       border={@Mods/StardewUI/Sprites/MenuBorder} border-thickness="36, 36, 40, 36">
  <scrollable layout="stretch stretch" peeking="128">
    <grid layout="stretch content"
          item-layout="length: 160"
          horizontal-item-alignment="middle">
      <frame *repeat={:LivestockData} padding="16" background={@mushymato.LivestockBazaar/sprites/cursors:ShopBg}>
        <lane layout="128px 128px" orientation="vertical" horizontal-content-alignment="middle">
          <image layout="content 64px" sprite={:ShopIcon} />
          <label layout="content 64px" text={:ShopDisplayName} />
        </lane>
      </frame>
    </grid>
  </scrollable>
</frame>
