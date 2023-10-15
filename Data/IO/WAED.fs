namespace IO.WAED

type MGEFJson =
    { name: string
      edid: string
      description: string }

type MGEFJsonDB = Map<string, MGEFJson>
