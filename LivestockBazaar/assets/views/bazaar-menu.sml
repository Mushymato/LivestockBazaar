<lane orientation="horizontal" >
  <!-- shop owner portrait -->
  <lane *if={:IsWidescreen} layout="320px content" padding="0,8,0,0" orientation="vertical" horizontal-content-alignment="end">
    <frame *if={:ShowPortraitBox} padding="20" background={:Theme_PortraitBackground}>
      <image layout="256px 256px" sprite={:OwnerPortrait} />
    </frame>
    <frame layout="256px content[..320]" padding="20" margin="0,10" border={:Theme_DialogueBackground}>
      <label font="dialogue" text={:OwnerDialog} color={:Theme_DialogueColor} shadow-color={:Theme_DialogueShadowColor}/>
    </frame>
  </lane>

  <!-- main body -->
  <frame *switch={CurrentPage}
         layout={:MainBodyLayout} border={:Theme_WindowBorder}
         border-thickness={:Theme_WindowBorderThickness} margin="8">
    <!-- page 1 -->
    <lane *case="1" layout="stretch content" orientation="horizontal">
      <!-- for sale -->
      <scrollable-styled>
        <grid layout="stretch content" item-layout="length: 192"
              horizontal-item-alignment="middle">
          <frame *repeat={:LivestockEntries}
            padding="16"
            background={:^Theme_ItemRowBackground}
            background-tint={BackgroundTint}
            pointer-enter=|^HandleHoverLivestock(this)|
            pointer-leave=|^HandleHoverLivestock()|
            left-click=|^HandleSelectLivestock(this)| >
            <panel opacity={:ShopIconOpacity} layout="160px 144px" horizontal-content-alignment="middle" focusable="true">
              <image layout="content 64px" margin="0,8" sprite={:ShopIcon} tint={:ShopIconTint}/>
              <lane layout="stretch 64px" margin="0,88" orientation="horizontal">
                <image layout="48px 48px" sprite={:TradeItem} />
                <label layout="stretch 48px" text={:TradePrice} font={:TradeDisplayFont} />
              </lane>
            </panel>
          </frame>
        </grid>
      </scrollable-styled>
      <!-- info box -->
      <infobox *context={HoveredLivestock} />
    </lane>
    <!-- page 2 -->
    <lane *case="2" *context={SelectedLivestock} layout="stretch content" orientation="horizontal">
      <lane layout={:^ForSaleLayout} orientation="vertical">
        <lane orientation="horizontal">
          <image
            sprite={@mushymato.LivestockBazaar/sprites/cursors:dice}
            margin="12" left-click=|RandomizeBuyName()| 
          />
          <textinput layout="128px 64px" text={<>BuyName}/>
        </lane>
        <scrollable-styled>
        <lane orientation="vertical">
          <lane *repeat={:ValidAnimalHouseLocations} layout="stretch content" orientation="vertical">
              <banner padding="8" text={:LocationName} />
              <lane orientation="vertical">
                <frame *repeat={:ValidLivestockBuildings}
                  focusable="true" padding="12" layout="stretch content"
                  background={:^^^Theme_ItemRowBackground}
                  background-tint={BackgroundTint}
                  pointer-enter=|^^^HandleHoverBuilding(this)|
                  pointer-leave=|^^^HandleHoverBuilding()|
                  left-click=|^^^HandlePurchaseAnimal(this)| >
                  <label padding="8" font="dialogue" text={BuildingName}/>
                </frame>
              </lane>
          </lane>
        </lane>
        </scrollable-styled>
      </lane>
      <infobox/>
    </lane>
  </frame>

  <!-- spacer to counterbalance the portrait -->
  <spacer *if={:IsWidescreen} layout="192px 0px"/>
</lane>

<template name="infobox">
  <lane layout="256px stretch"  orientation="vertical" horizontal-content-alignment="middle">
    <panel layout="content content[128..]" margin="8,8,0,0" horizontal-content-alignment="middle" vertical-content-alignment="end">
      <image layout={:AnimLayout} sprite={AnimSprite} sprite-effects={AnimFlip} tint={ShopIconTint} />
      <!-- <image layout={:AnimLayout} sprite={AnimSprite} tint={ShopIconTint} /> -->
    </panel>
    <label *if={HasValidHouse} text={:LivestockName} font="dialogue"/>
    <label *if={HasValidHouse} text={:Description} font="small" margin="8,0" />
    <outlet/>
  </lane>
</template>

<template name="scrollable-styled">
  <scrollable layout={:~BazaarContextMain.ForSaleLayout} peeking="128"
              scrollbar-margin="278,0,0,-8"
              scrollbar-up-sprite={:~BazaarContextMain.Theme_ScrollUp}
              scrollbar-down-sprite={:~BazaarContextMain.Theme_ScrollDown}
              scrollbar-down-sprite={:~BazaarContextMain.Theme_ScrollDown}
              scrollbar-thumb-sprite={:~BazaarContextMain.Theme_ScrollBarFront}
              scrollbar-track-sprite={:~BazaarContextMain.Theme_ScrollBarBack}>
    <outlet/>
  </scrollable>
</template>
